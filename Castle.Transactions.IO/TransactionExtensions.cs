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
}