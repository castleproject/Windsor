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

namespace Castle.Services.vNextTransaction
{
	internal class TxManager : ITxManager, ITxManagerInternal
	{
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

		Maybe<ITransaction> ITxManager.CreateTransaction(ITransactionOptions transactionOptions)
		{
			var activity = _ActivityManager.GetCurrentActivity();
			
			if (transactionOptions.Mode == TransactionScopeOption.Suppress)
				return Maybe.None<ITransaction>();

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
			var m = Maybe.Some(tx);

			// assume because I can't make value types and reference types equal enough
			// and boxing doesn't do it for me.
			Contract.Assume(m.HasValue && m.Value.State == TransactionState.Active);

			return m;
		}

		void IDisposable.Dispose()
		{
		}
	}
}