namespace Castle.Transactions.IO
{
	using System;

	using Castle.Transactions.Internal;

	public static class TransactionExtensions
	{
			//Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>() != null
			//                 && (!Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			//                     || Contract.Result<Maybe<ICreatedTransaction>>().Value.Transaction.State == TransactionState.Active));

		public static Maybe<IFileTransaction> CreateFileTransaction(this ITransactionManager transactionManager)
		{
			return CreateFileTransaction(transactionManager, new DefaultTransactionOptions());
		}

		public static Maybe<IFileTransaction> CreateFileTransaction(this ITransactionManager transactionManager, ITransactionOptions transactionOptions)
		{
			if (transactionOptions.Fork)
				throw new NotImplementedException("forking file transactions not implemented");

			// TODO: we need to decide what transaction manager we want running the show and be smarter about this:
			var activity = transactionManager.Activities.GetCurrentActivity();
			IFileTransaction tx = new FileTransaction();
			var fork = transactionOptions.ShouldFork(activity.Count + 1);
			if (!fork) activity.Push(tx);
			return Maybe.Some(tx);
			//return new CreatedTransaction(tx, fork, transactionManager.ForkScopeFactory(tx));
		}
	}

	/// <summary>
	/// Interface for file transactions.
	/// </summary>
	public interface IFileTransaction : ITransaction
	{
		/// <summary>
		/// Gets the safe file transaction handle.
		/// </summary>
		Maybe<SafeKernelTransactionHandle> Handle { get; }
	}
}