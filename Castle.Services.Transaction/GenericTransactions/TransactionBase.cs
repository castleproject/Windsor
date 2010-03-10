using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core;
using log4net;

namespace Castle.Services.Transaction
{
	public abstract class TransactionBase : MarshalByRefObject, ITransaction, IDisposable
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TransactionBase));
		private readonly ReaderWriterLockSlim _Sem = new ReaderWriterLockSlim();
		private readonly IList<IResource> _Resources = new List<IResource>();
		private readonly IList<ISynchronization> _SyncInfo = new List<ISynchronization>();

		private volatile bool _CanCommit;

		private readonly TransactionMode _TransactionMode;
		private readonly IsolationMode _IsolationMode;

		internal readonly string _TheName;

		protected TransactionBase(string name, TransactionMode mode, IsolationMode isolationMode)
		{
			_TheName = name ?? string.Empty;
			_TransactionMode = mode;
			_IsolationMode = isolationMode;
			Status = TransactionStatus.NoTransaction;
			Context = new Hashtable();
		}

		#region Nice-to-have properties

		/// <summary>
		/// Returns the current transaction status.
		/// </summary>
		public TransactionStatus Status { get; private set; }

		/// <summary>
		/// Transaction context. Can be used by applications.
		/// </summary>
		public IDictionary Context { get; private set; }

		///<summary>
		/// Gets whether the transaction is a child transaction or not.
		///</summary>
		public virtual bool IsChildTransaction { get { return false; } }

		public ChildTransaction CreateChildTransaction()
		{
			// opposite to what old code things, I don't think we need
			// to have a list of child transactions since we never use them.
			return new ChildTransaction(this);
		}

		/// <summary>
		/// <see cref="ITransaction.IsAmbient"/>.
		/// </summary>
		public abstract bool IsAmbient { get; protected set; }

		/// <summary>
		/// Gets whether rollback only is set.
		/// </summary>
		public bool IsRollbackOnlySet
		{
			get { return !_CanCommit; }
		}

		///<summary>
		/// Gets the transaction mode of the transaction.
		///</summary>
		public TransactionMode TransactionMode
		{
			get { return _TransactionMode; }
		}

		///<summary>
		/// Gets the isolation mode of the transaction.
		///</summary>
		public IsolationMode IsolationMode
		{
			get { return _IsolationMode; }
		}

		///<summary>
		/// Gets the name of the transaction.
		///</summary>
		public virtual string Name 
		{ 
			get { return string.IsNullOrEmpty(_TheName) ? 
			                                         	string.Format("Tx #{0}", GetHashCode()) : _TheName; }
		}

		#endregion

		/// <summary>
		/// <see cref="ITransaction.Begin"/>.
		/// </summary>
		public virtual void Begin()
		{
			_Sem.AtomWrite(() =>
			{
				AssertState(TransactionStatus.NoTransaction);
				Status = TransactionStatus.Active;
			});

			_Logger.TryLogFail(InnerBegin)
				.Exception(e => { _CanCommit = false; throw new TransactionException("Could not begin transaction.", e); })
				.Success(() => _CanCommit = true);

			_Sem.AtomRead(() =>
			{
				foreach (var r in _Resources)
				{
					try { r.Start(); }
					catch (Exception e)
					{
						SetRollbackOnly();
						throw new CommitResourceException("Transaction could not commit because of a failed resource.",
							e, r);
					}
				}
			});
		}

		/// <summary>
		/// Implementors set <see cref="Status"/>.
		/// </summary>
		protected abstract void InnerBegin();

		/// <summary>
		/// Succeed the transaction, persisting the
		///             modifications
		/// </summary>
		public virtual void Commit()
		{
			if (!_CanCommit) throw new TransactionException("Rollback only was set.");

			_Sem.AtomWrite(() =>
			{
				AssertState(TransactionStatus.Active);
				Status = TransactionStatus.Committed;
			});

			_Sem.AtomRead(() => {

				_SyncInfo.ForEach(s => _Logger.TryLogFail(s.BeforeCompletion));

            	foreach (var r in _Resources)
            	{
					try { r.Commit(); } 
					catch (Exception e)
					{
						SetRollbackOnly();
						throw new CommitResourceException("Transaction could not commit because of a failed resource.", 
							e, r);
					}
				}
			});

			try
			{
				_Logger.TryLogFail(InnerCommit)
					.Exception(e => { throw new TransactionException("Could not commit", e); });
			}
			finally
			{
				_Sem.AtomRead(() => _SyncInfo.ForEach(s => _Logger.TryLogFail(s.AfterCompletion)));
			}
		}

		/// <summary>
		/// Implementors should NOT change the base class.
		/// </summary>
		protected abstract void InnerCommit();

		/// <summary>
		/// See <see cref="ITransaction.Rollback"/>.
		/// </summary>
		public virtual void Rollback()
		{
			_Sem.AtomWrite(() =>
			{
				AssertState(TransactionStatus.Active);
				Status = TransactionStatus.RolledBack;
				_CanCommit = false;
			});

			var failures = new List<Pair<IResource, Exception>>();

			Exception toThrow = null;

			_SyncInfo.ForEach(s => _Logger.TryLogFail(s.BeforeCompletion));

			_Logger
				.TryLogFail(InnerRollback)
				.Exception(e => toThrow = e);

			try
			{
				_Sem.AtomRead(() =>
				{
					_Resources.ForEach(r =>
						_Logger.TryLogFail(r.Rollback)
							.Exception(e => failures.Add(r.And(e))));

					if (failures.Count == 0) return;

					if (toThrow == null)
						throw new RollbackResourceException(
							"Failed to properly roll back all resources. See the inner exception for details",
							failures);
					
					throw toThrow;
				});
			}
			finally
			{
				_Sem.AtomRead(() => _SyncInfo.ForEach(s => _Logger.TryLogFail(s.AfterCompletion)));
			}
		}

		/// <summary>
		/// Implementors should NOT change the base class.
		/// </summary>
		protected abstract void InnerRollback();

		/// <summary>
		/// Signals that this transaction can only be rolledback. 
		///             This is used when the transaction is not being managed by
		///             the callee.
		/// </summary>
		public virtual void SetRollbackOnly()
		{
			_CanCommit = false;
		}

		#region Resources

		/// <summary>
		/// Register a participant on the transaction.
		/// </summary>
		/// <param name="resource"/>
		public virtual void Enlist(IResource resource)
		{
			if (resource == null) throw new ArgumentNullException("resource");
			_Sem.AtomWrite(() => {
             	if (_Resources.Contains(resource)) return;
             	if (Status == TransactionStatus.Active)
					_Logger.TryLogFail(resource.Start).Exception(_ => SetRollbackOnly());
             	_Resources.Add(resource);
			});
		}

		/// <summary>
		/// Registers a synchronization object that will be 
		///             invoked prior and after the transaction completion
		///             (commit or rollback)
		/// </summary>
		/// <param name="s"/>
		public void RegisterSynchronization(ISynchronization s)
		{
			if (s == null) throw new ArgumentNullException("s");

			_Sem.AtomWrite(() =>
			{
				if (_SyncInfo.Contains(s)) return;
				_SyncInfo.Add(s);
			});
		}

		public IEnumerable<IResource> Resources()
		{
			foreach (var resource in _Resources.ToList())
				yield return resource;
		}

		#endregion

		#region utils

		protected void AssertState(TransactionStatus status)
		{
			AssertState(status, null);
		}

		protected void AssertState(TransactionStatus status, string msg)
		{
			if (status != Status)
			{
				if (!string.IsNullOrEmpty(msg))
					throw new TransactionException(msg);

				throw new TransactionException(string.Format("State failure; should have been {0} but was {1}",
				                                             status, Status));
			}
		}

		#endregion

		public virtual void Dispose()
		{
			_Sem.AtomWrite(() =>
			               	{
			               		_Resources.Select(r => r as IDisposable)
									.Where(r => r != null)
									.ForEach(r => r.Dispose());

								_Resources.Clear();
								_SyncInfo.Clear();
			               	});

		}
	}
}