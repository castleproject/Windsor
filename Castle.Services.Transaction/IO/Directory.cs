using System;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction.IO
{
	public static class Directory
	{
		private static IDirectoryAdapter GetAdapter()
		{
			return new DirectoryAdapter(new MapPathImpl(), false, null);
		}

		public static bool Create(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().Create(path);
		}

		public static bool Exists(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().Exists(path);
		}

		public static void Delete(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			GetAdapter().Delete(path);
		}

		public static bool Delete(string path, bool recursively)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().Delete(path, recursively);
		}

		public static string GetFullPath(string dir)
		{
			Contract.Requires(!string.IsNullOrEmpty(dir));
			return GetAdapter().GetFullPath(dir);
		}

		public static string MapPath(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().MapPath(path);
		}

		public static void Move(string originalPath, string newPath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalPath));
			Contract.Requires(!string.IsNullOrEmpty(newPath));
			GetAdapter().Move(originalPath, newPath);
		}

		public static bool CreateDirectory(string directoryPath)
		{
			return Create(directoryPath);
		}
	}
}