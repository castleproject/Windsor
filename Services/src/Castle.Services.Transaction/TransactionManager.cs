// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Services.Transaction
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Threading.Tasks;
	using System.Transactions;

	using Castle.Core.Logging;
	using Castle.Services.Transaction.Internal;

	public sealed class TransactionManager : ITransactionManager
	{
		private readonly IActivityManager activityManager;
		private ILogger logger = NullLogger.Instance;

		/// <summary>
		/// Gets or sets the logger - this is set through inversion
		/// of control.
		/// </summary>
		public ILogger Logger
		{
			get { return logger; }
			set { logger = value; }
		}

		public TransactionManager(IActivityManager activityManager)
		{
			Contract.Requires(activityManager != null);
			this.activityManager = activityManager;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(activityManager != null);
		}

		Maybe<ITransaction> ITransactionManager.CurrentTopTransaction
		{
			get { return activityManager.GetCurrentActivity().TopTransaction; }
		}

		Maybe<ITransaction> ITransactionManager.CurrentTransaction
		{
			get { return activityManager.GetCurrentActivity().CurrentTransaction; }
		}

		uint ITransactionManager.Count
		{
			get { return activityManager.GetCurrentActivity().Count; }
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "CommittableTransaction is disposed by Transaction")]
		Maybe<ICreatedTransaction> ITransactionManager.CreateTransaction()
		{
			return ((ITransactionManager)this).CreateTransaction(new DefaultTransactionOptions());
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "CommittableTransaction is disposed by Transaction")]
		Maybe<ICreatedTransaction> ITransactionManager.CreateTransaction(ITransactionOptions transactionOptions)
		{
			var activity = activityManager.GetCurrentActivity();

			if (transactionOptions.Mode == TransactionScopeOption.Suppress)
				return Maybe.None<ICreatedTransaction>();

			var nextStackDepth = activity.Count + 1;
			var shouldFork = transactionOptions.Fork && nextStackDepth > 1;

			ITransaction tx;
			if (activity.Count == 0)
				tx = new Transaction(new CommittableTransaction(new TransactionOptions
				{
					IsolationLevel = transactionOptions.IsolationLevel,
					Timeout = transactionOptions.Timeout
				}), nextStackDepth, transactionOptions, () => activity.Pop(), logger.CreateChildLogger("Transaction"));
			else
			{
				var clone = activity
					.CurrentTransaction.Value
					.Inner
					.DependentClone(transactionOptions.DependentOption);
				Contract.Assume(clone != null);

				Action onDispose = () => activity.Pop();
				tx = new Transaction(clone, nextStackDepth, transactionOptions, shouldFork ? null : onDispose,
				                     logger.CreateChildLogger("Transaction"));
			}

			if (!shouldFork) // forked transactions should not be on the current context's activity stack
				activity.Push(tx);

			Contract.Assume(tx.State == TransactionState.Active, "by c'tor post condition for both cases of the if statement");

			var m = Maybe.Some(new CreatedTransaction(tx,
				shouldFork, // we should only fork if we have a different current top transaction than the current
				() => {
					activityManager.GetCurrentActivity().Push(tx);
					return new DisposableScope(activityManager.GetCurrentActivity().Pop);
				}) as ICreatedTransaction);

			// warn if fork and the top transaction was just created
			if (transactionOptions.Fork && nextStackDepth == 1 && logger.IsWarnEnabled)
				logger.WarnFormat("transaction {0} created with Fork=true option, but was top-most "
				                  + "transaction in invocation chain. running transaction sequentially",
				                  tx.LocalIdentifier);

			Contract.Assume(m.HasValue && m.Value.Transaction.State == TransactionState.Active);

			return m;
		}

		Maybe<ICreatedTransaction> ITransactionManager.CreateFileTransaction(ITransactionOptions transactionOptions)
		{
			throw new NotImplementedException("Implement file transactions in the transaction manager!");
		}

		/// <summary>
		/// 	Enlists a dependent task in the current top transaction.
		/// </summary>
		/// <param name = "task">
		/// 	The task to enlist; this task is the action of running
		/// 	a dependent transaction on the thread pool.
		/// </param>
		public void EnlistDependentTask(Task task)
		{
			Contract.Requires(task != null);
			activityManager.GetCurrentActivity().EnlistDependentTask(task);
		}

		private class DisposableScope : IDisposable
		{
			private readonly Func<ITransaction> _OnDispose;

			public DisposableScope(Func<ITransaction> onDispose)
			{
				Contract.Requires(onDispose != null);
				_OnDispose = onDispose;
			}

			public void Dispose()
			{
				_OnDispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool isManaged)
		{
			if (!isManaged)
				return;
		}
	}
}