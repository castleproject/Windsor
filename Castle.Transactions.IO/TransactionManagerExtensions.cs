using System;

namespace Castle.Transactions.IO
{
	public static class TransactionManagerExtensions
	{
		public static Maybe<ITransaction> CreateFileTransaction(this ITransactionManager manager,
		                                                        ITransactionOptions options)
		{
			throw new NotImplementedException();
		}
	}
}