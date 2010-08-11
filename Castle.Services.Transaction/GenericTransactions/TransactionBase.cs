namespace Castle.Services.Transaction
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Castle.Core;
	using log4net;
	using System.Transactions;

	public abstract class TransactionBase : MarshalByRefObject, ITransaction, IDisposable
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof (TransactionBase));

		private readonly ReaderWriterLockSlim sem = new ReaderWriterLockSlim();
		private readonly IList<IResource> resources = new List<IResource>();
		private readonly IList<ISynchronization> syncInfo = new List<ISynchronization>();

		private volatile bool canCommit;

		private readonly TransactionMode transactionMode;
		private readonly IsolationMode isolationMode;

		internal readonly string theName;
		private TransactionScope ambientTransaction;

		protected TransactionBase(string name, TransactionMode mode, IsolationMode isolationMode)
		{
			theName = name ?? string.Empty;
			transactionMode = mode;
			this.isolationMode = isolationMode;
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
		public virtual bool IsRollbackOnlySet
		{
			get { return !canCommit; }
		}

		///<summary>
		/// Gets the transaction mode of the transaction.
		///</summary>
		public TransactionMode TransactionMode
		{
			get { return transactionMode; }
		}

		///<summary>
		/// Gets the isolation mode of the transaction.
		///</summary>
		public IsolationMode IsolationMode
		{
			get { return isolationMode; }
		}

		///<summary>
		/// Gets the name of the transaction.
		///</summary>
		public virtual string Name 
		{ 
			get { return string.IsNullOrEmpty(theName) ? 
			                                         	string.Format("Tx #{0}", GetHashCode()) : theName; }
		}

		#endregion

		/// <summary>
		/// <see cref="ITransaction.Begin"/>.
		/// </summary>
		public virtual void Begin()
		{
			sem.AtomWrite(() =>
			{
				AssertState(TransactionStatus.NoTransaction);
				Status = TransactionStatus.Active;
			});

			logger.TryLogFail(InnerBegin)
				.Exception(e => { canCommit = false; throw new TransactionException("Could not begin transaction.", e); })
				.Success(() => 
					canCommit = true);

			sem.AtomRead(() =>
			{
				foreach (var r in resources)
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
			if (!canCommit) throw new TransactionException("Rollback only was set.");

			sem.AtomWrite(() =>
			{
				AssertState(TransactionStatus.Active);
				Status = TransactionStatus.Committed;
			});

			var commitFailed = false;
			
			try
			{
				sem.AtomRead(() => 
				{
					syncInfo.ForEach(s => logger.TryLogFail(s.BeforeCompletion));

					foreach (var r in resources)
					{
						try
						{
							logger.DebugFormat("Resource: " + r);

							r.Commit();
						} 
						catch (Exception e)
						{
							SetRollbackOnly();
							commitFailed = true;

							logger.ErrorFormat("Resource state: " + r);

							throw new CommitResourceException("Transaction could not commit because of a failed resource.", e, r);
						}
					}
				});

			
				logger
					.TryLogFail(InnerCommit)
					.Exception(e =>
					           	{
									commitFailed = true;

					           		throw new TransactionException("Could not commit", e);
					           	});
			}
			finally
			{
				if (!commitFailed)
				{
					if (ambientTransaction != null)
					{
						logger.DebugFormat("Commiting TransactionScope (Ambient Transaction) for '{0}'. ", Name);

						ambientTransaction.Complete();
						DisposeAmbientTx();
					}

					sem.AtomRead(() => syncInfo.ForEach(s => logger.TryLogFail(s.AfterCompletion)));
				}
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
			sem.AtomWrite(() =>
			{
				AssertState(TransactionStatus.Active);
				Status = TransactionStatus.RolledBack;
				canCommit = false;
			});

			var failures = new List<Pair<IResource, Exception>>();

			Exception toThrow = null;

			syncInfo.ForEach(s => logger.TryLogFail(s.BeforeCompletion));

			logger
				.TryLogFail(InnerRollback)
				.Exception(e => toThrow = e);

			try
			{
				sem.AtomRead(() =>
				{
					resources.ForEach(r =>
						logger.TryLogFail(r.Rollback)
							.Exception(e => failures.Add(r.And(e))));

					if (failures.Count == 0) return;

					if (toThrow == null)
						throw new RollbackResourceException(
							"Failed to properly roll back all resources. See the inner exception or the failed resources list for details",
							failures);
					
					throw toThrow;
				});
			}
			finally
			{
				if (ambientTransaction != null)
				{
					logger.DebugFormat("Rolling back TransactionScope (Ambient Transaction) for '{0}'. ", Name);

					DisposeAmbientTx();
				}

				sem.AtomRead(() => syncInfo.ForEach(s => logger.TryLogFail(s.AfterCompletion)));
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
			canCommit = false;
		}

		#region Resources

		/// <summary>
		/// Register a participant on the transaction.
		/// </summary>
		/// <param name="resource"/>
		public virtual void Enlist(IResource resource)
		{
			if (resource == null) throw new ArgumentNullException("resource");
			sem.AtomWrite(() => {
			                    	if (resources.Contains(resource)) return;
			                    	if (Status == TransactionStatus.Active)
			                    		logger.TryLogFail(resource.Start).Exception(_ => SetRollbackOnly());
			                    	resources.Add(resource);
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

			sem.AtomWrite(() =>
			              	{
			              		if (syncInfo.Contains(s)) return;
			              		syncInfo.Add(s);
			              	});
		}

		public IEnumerable<IResource> Resources()
		{
			foreach (var resource in resources.ToList())
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
			sem.AtomWrite(() =>
			              	{
			              		resources.Select(r => r as IDisposable)
			              			.Where(r => r != null)
			              			.ForEach(r => r.Dispose());

			              		resources.Clear();
			              		syncInfo.Clear();
			              	});

			if (ambientTransaction != null)
			{
				DisposeAmbientTx();
			}
		}

		private void DisposeAmbientTx()
		{
			ambientTransaction.Dispose();
			ambientTransaction = null;
		}

		public void CreateAmbientTransaction()
		{
			ambientTransaction = new TransactionScope();

			logger.DebugFormat("Created a TransactionScope (Ambient Transaction) for '{0}'. ", Name);
		}
	}
}