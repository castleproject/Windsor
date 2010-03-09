using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core;
using log4net;

namespace Castle.Services.Transaction
{
	public abstract class TransactionBase : MarshalByRefObject, ITransaction
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TransactionBase));
		private readonly ReaderWriterLockSlim _Sem = new ReaderWriterLockSlim();
		private readonly IList<IResource> _Resources = new List<IResource>();
		private readonly IList<ISynchronization> _SyncInfo = new List<ISynchronization>();

		private bool _CanCommit;

		private readonly TransactionMode _TransactionMode;
		private readonly IsolationMode _IsolationMode;

		protected readonly string _Name;

		protected TransactionBase(string name, TransactionMode mode, IsolationMode isolationMode)
		{
			_Name = name ?? string.Empty;
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
		/// Not relevant.
		///</summary>
		public virtual bool IsChildTransaction { get { return false; } }

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
			get { return string.IsNullOrEmpty(_Name) ? 
			                                         	string.Format("Tx #{0}", GetHashCode()) : _Name; }
		}

		#endregion

		/// <summary>
		/// <see cref="ITransaction.Begin"/>.
		/// </summary>
		public void Begin()
		{
			AssertState(TransactionStatus.NoTransaction);
			Status = TransactionStatus.Active;

			_Logger.TryAndLog(InnerBegin)
				.Exception(e => { _CanCommit = false; throw new TransactionException("Could not start transaction.", e); })
				.Success(() => _CanCommit = true);
		}

		/// <summary>
		/// Implementors set <see cref="Status"/>.
		/// </summary>
		internal abstract void InnerBegin();

		/// <summary>
		/// Succeed the transaction, persisting the
		///             modifications
		/// </summary>
		public void Commit()
		{
			if (!_CanCommit) throw new TransactionException("Rollback only was set.");
			AssertState(TransactionStatus.Active);
			Status = TransactionStatus.Committed;

			_Sem.AtomRead(() => {
				_Resources.ForEach(r => r.Commit());
				_SyncInfo.ForEach(s => _Logger.TryAndLog(s.BeforeCompletion));
			});

			_Logger.TryAndLog(InnerCommit).Exception(e =>
			{
				throw new TransactionException("Could not commit", e);
			});

			_Sem.AtomRead(() => _SyncInfo.ForEach(s => _Logger.TryAndLog(s.AfterCompletion)));
		}

		/// <summary>
		/// Implementors should NOT change the base class.
		/// </summary>
		internal abstract void InnerCommit();

		/// <summary>
		/// See <see cref="ITransaction.Rollback"/>.
		/// </summary>
		public void Rollback()
		{
			AssertState(TransactionStatus.Active);
			Status = TransactionStatus.RolledBack;
			_CanCommit = false;
			var resources = new List<Pair<IResource, Exception>>();

			Exception toThrow = null;

			_Logger
				.TryAndLog(InnerRollback)
				.Exception(e => toThrow = e);

			try
			{
				_Sem.AtomRead(() =>
				{
					_Resources.ForEach(r =>
						_Logger.TryAndLog(r.Rollback)
							.Exception(e => resources.Add(r.And(e))));

					if (resources.Count == 0) return;

					var lastR = resources[resources.Count - 1];

					if (toThrow == null)
						throw new RollbackResourceException("Failed to properly dispose all resources. See the inner exception for details",
														lastR.Second, lastR.First, resources.Select(r => r.First).ToArray());
				});
			}
			finally
			{
				_Sem.AtomRead(() => _SyncInfo.ForEach(s => _Logger.TryAndLog(s.AfterCompletion)));
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
		public void SetRollbackOnly()
		{
			_CanCommit = false;
		}

		/// <summary>
		/// Register a participant on the transaction.
		/// </summary>
		/// <param name="resource"/>
		public void Enlist(IResource resource)
		{
			if (resource == null) throw new ArgumentNullException("resource");
			_Sem.AtomWrite(() => {
				if (_Resources.Contains(resource)) return;
				_Logger.TryAndLog(resource.Start)
					.Exception(_ => SetRollbackOnly());
				_Resources.Add(resource);
			});
		}

		/// <summary>
		/// Registers a synchronization object that will be 
		///             invoked prior and after the transaction completion
		///             (commit or rollback)
		/// </summary>
		/// <param name="synchronization"/>
		public void RegisterSynchronization(ISynchronization synchronization)
		{
			if (synchronization == null) throw new ArgumentNullException("synchronization");
			_Sem.AtomWrite(() => _SyncInfo.Add(synchronization));
		}

		public IEnumerable<IResource> Resources()
		{
			foreach (var resource in _Resources.ToList()) // note: not really thread safe.
				yield return resource;
		}

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
	}
}