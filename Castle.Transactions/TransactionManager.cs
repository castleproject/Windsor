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

namespace Castle.Transactions
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Threading.Tasks;
	using System.Transactions;

	using Castle.Transactions.Internal;

	using NLog;

	/// <summary>
	/// 	The default transaction manager that is capable of handling most combinations
	/// 	of <see cref = "ITransactionOptions" />.
	/// </summary>
	public class TransactionManager : ITransactionManager
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IActivityManager activityManager;

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

		IActivityManager ITransactionManager.Activities
		{
			get { return activityManager; }
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

		Maybe<ICreatedTransaction> ITransactionManager.CreateTransaction()
		{
			return ((ITransactionManager)this).CreateTransaction(new DefaultTransactionOptions());
		}

		Maybe<ICreatedTransaction> ITransactionManager.CreateTransaction(ITransactionOptions transactionOptions)
		{
			var activity = activityManager.GetCurrentActivity();

			if (transactionOptions.Mode == TransactionScopeOption.Suppress)
				return Maybe.None<ICreatedTransaction>();

			var nextStackDepth = activity.Count + 1;
			var shouldFork = transactionOptions.ShouldFork(nextStackDepth);

			ITransaction tx;
			if (activity.Count == 0)
				tx = new Transaction(new CommittableTransaction(new TransactionOptions
				{
					IsolationLevel = transactionOptions.IsolationLevel,
					Timeout = transactionOptions.Timeout
				}), nextStackDepth, transactionOptions, () => activity.Pop());
			else
			{
				var clone = activity
					.CurrentTransaction.Value
					.Inner
					.DependentClone(transactionOptions.DependentOption);
				Contract.Assume(clone != null);

				Action onDispose = () => activity.Pop();
				tx = new Transaction(clone, nextStackDepth, transactionOptions, shouldFork ? null : onDispose);
			}

			if (!shouldFork) // forked transactions should not be on the current context's activity stack
				activity.Push(tx);

			Contract.Assume(tx.State == TransactionState.Active, "by c'tor post condition for both cases of the if statement");

			// we should only fork if we have a different current top transaction than the current
			var m = Maybe.Some(new CreatedTransaction(tx, shouldFork, this.ForkScopeFactory(tx)) as ICreatedTransaction);

			// warn if fork and the top transaction was just created
			if (transactionOptions.Fork && nextStackDepth == 1)
				logger.Warn("transaction {0} created with Fork=true option, but was top-most "
				            + "transaction in invocation chain. running transaction sequentially",
				            tx.LocalIdentifier);

			Contract.Assume(m.HasValue && m.Value.Transaction.State == TransactionState.Active);

			return m;
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

		public class DisposableScope : IDisposable
		{
			private readonly Func<ITransaction> onDispose;

			public DisposableScope(Func<ITransaction> onDispose)
			{
				Contract.Requires(onDispose != null);
				this.onDispose = onDispose;
			}

			public void Dispose()
			{
				onDispose();
			}
		}

		public void Dispose()
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