#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Transactions;
using Castle.Services.Transaction.Internal;
using log4net;

namespace Castle.Services.Transaction
{
	public class TxManager : ITxManager
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TxManager));
		private readonly IActivityManager _ActivityManager;

		public TxManager(IActivityManager activityManager)
		{
			Contract.Requires(activityManager != null);
			Contract.Ensures(_ActivityManager != null);
			_ActivityManager = activityManager;
		}

		Maybe<ITransaction> ITxManager.CurrentTopTransaction
		{
			get { return _ActivityManager.GetCurrentActivity().TopTransaction; }
		}

		Maybe<ITransaction> ITxManager.CurrentTransaction
		{
			get { return _ActivityManager.GetCurrentActivity().CurrentTransaction; }
		}

		uint ITxManager.Count
		{
			get { return _ActivityManager.GetCurrentActivity().Count; }
		}

		void ITxManager.AddRetryPolicy(string policyKey, Func<Exception, bool> retryPolicy)
		{
			throw new NotImplementedException();
		}

		void ITxManager.AddRetryPolicy(string policyKey, IRetryPolicy retryPolicy)
		{
			throw new NotImplementedException();
		}

		Maybe<ICreatedTransaction> ITxManager.CreateTransaction(ITransactionOptions transactionOptions)
		{
			var activity = _ActivityManager.GetCurrentActivity();
			
			if (transactionOptions.Mode == TransactionScopeOption.Suppress)
				return Maybe.None<ICreatedTransaction>();

			var nextStackDepth = activity.Count + 1;

			ITransaction tx;
			if (activity.Count == 0)
			{
				tx = new Transaction(new CommittableTransaction(new TransactionOptions
				{
					IsolationLevel = transactionOptions.IsolationLevel,
					Timeout = transactionOptions.Timeout
				}), nextStackDepth, transactionOptions, () => activity.Pop());
			}
			else
			{
				var clone = activity
					.CurrentTransaction.Value
					.Inner
					.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
				
				// assume because I can't open up .Net and add the contract myself
				Contract.Assume(clone != null);
				tx = new Transaction(clone, nextStackDepth, transactionOptions, () => activity.Pop());
			}

			activity.Push(tx);

			var m = Maybe.Some((ICreatedTransaction) new CreatedTransaction(tx,
				// we should only fork if we have a different current top transaction than the current
				transactionOptions.Fork && nextStackDepth > 1, () => {
					_ActivityManager.GetCurrentActivity().Push(tx);
					return new DisposableScope(_ActivityManager.GetCurrentActivity().Pop);
				}));

			// warn if fork and the top transaction was just created
			if (transactionOptions.Fork && nextStackDepth == 1)
				_Logger.WarnFormat("transaction {0} created with Fork=true option, but was top-most "
						+ "transaction in invocation chain. running transaction sequentially",
						tx.LocalIdentifier);

			// assume because I can't make value types and reference types equal enough
			// and boxing doesn't do it for me.
			Contract.Assume(m.HasValue && m.Value.Transaction.State == TransactionState.Active);

			return m;
		}

		public Maybe<ICreatedTransaction> CreateFileTransaction(ITransactionOptions transactionOptions)
		{
			throw new NotImplementedException();
		}

		private class DisposableScope : IDisposable
		{
			private readonly Func<ITransaction> _OnDispose;

			public DisposableScope(Func<ITransaction> onDispose)
			{
				Contract.Requires(onDispose != null);
				_OnDispose = onDispose;
			}

			#region Implementation of IDisposable

			public void Dispose()
			{
				_OnDispose();
			}

			#endregion
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isManaged)
		{
			if (!isManaged)
				return;
		}
	}
}