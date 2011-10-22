using System.Transactions;

namespace Castle.Transactions.IO
{
	using System;

	/// <summary>
	/// Attribute denoting a file transaction should be started at the entry
	/// of this method.
	/// </summary>
	public sealed class FileTransactionAttribute : TransactionAttribute
	{
		public FileTransactionAttribute()
		{
		}

		[Obsolete("Deprecated, use System.Transactions.TransactionScopeOption enum, instead.")]
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