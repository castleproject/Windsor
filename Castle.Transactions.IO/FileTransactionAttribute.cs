using System.Transactions;

namespace Castle.Transactions.IO
{
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