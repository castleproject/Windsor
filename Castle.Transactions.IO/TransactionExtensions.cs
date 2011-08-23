using System;
using System.Transactions;
using Castle.IO;

namespace Castle.Transactions.IO
{
	public static class TransactionExtensions
	{
		public static IFileSystem UpgradeToFileTransaction(this ITransaction tx)
		{
			return null;
		}
	}

	public static class TransactionManagerExtensions
	{
		public static Maybe<ITransaction> CreateFileTransaction(this ITransactionManager manager,
		                                                        ITransactionOptions options)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class FileTransactionAttribute : TransactionAttribute
	{
		public FileTransactionAttribute()
		{
		}

		public FileTransactionAttribute(TransactionMode transactionMode) : base(transactionMode)
		{
		}

		public FileTransactionAttribute(TransactionScopeOption mode) : base(mode)
		{
		}

		public FileTransactionAttribute(TransactionScopeOption mode, IsolationLevel isolationLevel)
			: base(mode, isolationLevel)
		{
		}
	}
}