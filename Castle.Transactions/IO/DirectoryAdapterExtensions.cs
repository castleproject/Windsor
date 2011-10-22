using Castle.IO;

namespace Castle.Transactions.IO
{
	public static class DirectoryAdapterExtensions
	{
		public static void DeleteDirectory(this IDirectoryAdapter a, string path)
		{
		}
		public static bool DeleteDirectory(this IDirectoryAdapter a, string path, bool recursive)
		{
			return false;
		}
	}
}