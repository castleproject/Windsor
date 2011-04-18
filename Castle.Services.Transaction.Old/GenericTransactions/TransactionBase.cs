#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Castle.Core;
using log4net;

namespace Castle.Services.Transaction
{
	public abstract class TransactionBase : MarshalByRefObject, ITransaction, IDisposable
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TransactionBase));
		private readonly IsolationMode _IsolationMode;

		private readonly IList<IResource> _Resources = new List<IResource>();
		private readonly IList<ISynchronization> _SyncInfo = new List<ISynchronization>();

		protected readonly string theName;
		private readonly TransactionMode _TransactionMode;
		private TransactionScope _AmbientTransaction;
		private volatile bool _CanCommit;

		protected TransactionBase(string name, TransactionMode mode, IsolationMode isolationMode)
		{
			theName = name ?? string.Empty;
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
		public virtual bool IsChildTransaction
		{
			get { return false; }
		}

		/// <summary>
		/// <see cref="ITransaction.IsAmbient"/>.
		/// </summary>
		public abstract bool IsAmbient { get; protected set; }

		/// <summary>
		/// <see cref="ITransaction.IsReadOnly"/>.
		/// </summary>
		public abstract bool IsReadOnly { get; protected set; }

		/// <summary>
		/// Gets whether rollback only is set.
		/// </summary>
		public virtual bool IsRollbackOnlySet
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
			get
			{
				return string.IsNullOrEmpty(theName)
				       	? string.Format("Tx #{0}", GetHashCode())
				       	: theName;
			}
		}

		public ChildTransaction CreateChildTransaction()
		{
			// opposite to what old code things, I don't think we need
			// to have a list of child transactions since we never use them.
			return new ChildTransaction(this);
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			_Resources.Select(r => r as IDisposable)
				.Where(r => r != null)
				.ForEach(r => r.Dispose());

			_Resources.Clear();
			_SyncInfo.Clear();

			if (_AmbientTransaction != null)
			{
				DisposeAmbientTx();
			}
		}

		#endregion

		#region ITransaction Members

		/// <summary>
		/// <see cref="ITransaction.Begin"/>.
		/// </summary>
		public virtual void Begin()
		{
			AssertState(TransactionStatus.NoTransaction);
			Status = TransactionStatus.Active;

			_Logger.TryLogFail(InnerBegin)
				.Exception(e => { 
					_CanCommit = false;
					throw new TransactionException("Could not begin transaction.", e);
				})
				.Success(() => _CanCommit = true);

			foreach (var r in _Resources)
			{
				try
				{
					r.Start();
				}
				catch (Exception e)
				{
					SetRollbackOnly();
					throw new CommitResourceException("Transaction could not commit because of a failed resource.",
					                                  e, r);
				}
			}
		}

		/// <summary>
		/// Succeed the transaction, persisting the
		///             modifications
		/// </summary>
		public virtual void Commit()
		{
			if (!_CanCommit) throw new TransactionException("Rollback only was set.");

			AssertState(TransactionStatus.Active);
			Status = TransactionStatus.Committed;

			bool commitFailed = false;

			try
			{
				_SyncInfo.ForEach(s => _Logger.TryLogFail(s.BeforeCompletion));

				foreach (var r in _Resources)
				{
					try
					{
						_Logger.DebugFormat("Resource: " + r);

						r.Commit();
					}
					catch (Exception e)
					{
						SetRollbackOnly();
						commitFailed = true;

						_Logger.ErrorFormat("Resource state: " + r);

						throw new CommitResourceException("Transaction could not commit because of a failed resource.", e, r);
					}
				}

				_Logger
					.TryLogFail(InnerCommit)
					.Exception(e => {
						commitFailed = true;
						throw new TransactionException("Could not commit", e);
					});
			}
			finally
			{
				if (!commitFailed)
				{
					if (_AmbientTransaction != null)
					{
						_Logger.DebugFormat("Commiting TransactionScope (Ambient Transaction) for '{0}'. ", Name);

						_AmbientTransaction.Complete();
						DisposeAmbientTx();
					}

					_SyncInfo.ForEach(s => _Logger.TryLogFail(s.AfterCompletion));
				}
			}
		}

		/// <summary>
		/// See <see cref="ITransaction.Rollback"/>.
		/// </summary>
		public virtual void Rollback()
		{
			AssertState(TransactionStatus.Active);
			Status = TransactionStatus.RolledBack;
			_CanCommit = false;

			var failures = new List<Pair<IResource, Exception>>();

			Exception toThrow = null;

			_SyncInfo.ForEach(s => _Logger.TryLogFail(s.BeforeCompletion));

			_Logger
				.TryLogFail(InnerRollback)
				.Exception(e => toThrow = e);

			try
			{
				_Resources.ForEach(r =>
				                  _Logger.TryLogFail(r.Rollback)
				                  	.Exception(e => failures.Add(r.And(e))));

				if (failures.Count == 0) return;

				if (toThrow == null)
					throw new RollbackResourceException(
						"Failed to properly roll back all resources. See the inner exception or the failed resources list for details",
						failures);

				throw toThrow;
			}
			finally
			{
				if (_AmbientTransaction != null)
				{
					_Logger.DebugFormat("Rolling back TransactionScope (Ambient Transaction) for '{0}'. ", Name);

					DisposeAmbientTx();
				}

				_SyncInfo.ForEach(s => _Logger.TryLogFail(s.AfterCompletion));
			}
		}

		/// <summary>
		/// Signals that this transaction can only be rolledback. 
		///             This is used when the transaction is not being managed by
		///             the callee.
		/// </summary>
		public virtual void SetRollbackOnly()
		{
			_CanCommit = false;
		}

		#endregion

		#region Resources

		/// <summary>
		/// Register a participant on the transaction.
		/// </summary>
		/// <param name="resource"/>
		public virtual void Enlist(IResource resource)
		{
			if (resource == null) throw new ArgumentNullException("resource");
			if (_Resources.Contains(resource)) return;
			if (Status == TransactionStatus.Active)
				_Logger.TryLogFail(resource.Start).Exception(_ => SetRollbackOnly());
			_Resources.Add(resource);
		}

		/// <summary>
		/// Registers a synchronization object that will be 
		///             invoked prior and after the transaction completion
		///             (commit or rollback)
		/// </summary>
		/// <param name="s"/>
		public virtual void RegisterSynchronization(ISynchronization s)
		{
			if (s == null) throw new ArgumentNullException("s");

			if (_SyncInfo.Contains(s)) return;
			_SyncInfo.Add(s);
		}

		public IEnumerable<IResource> Resources()
		{
			foreach (IResource resource in _Resources.ToList())
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

		/// <summary>
		/// Implementors set <see cref="Status"/>.
		/// </summary>
		protected abstract void InnerBegin();

		/// <summary>
		/// Implementors should NOT change the base class.
		/// </summary>
		protected abstract void InnerCommit();

		/// <summary>
		/// Implementors should NOT change the base class.
		/// </summary>
		protected abstract void InnerRollback();

		private void DisposeAmbientTx()
		{
			_AmbientTransaction.Dispose();
			_AmbientTransaction = null;
		}

		public void CreateAmbientTransaction()
		{
			_AmbientTransaction = new TransactionScope();

			_Logger.DebugFormat("Created a TransactionScope (Ambient Transaction) for '{0}'. ", Name);
		}
	}
}