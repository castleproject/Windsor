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
			return GetAdapter().Create(path);
		}

		public static bool Exists(string path)
		{
			return GetAdapter().Exists(path);
		}

		public static void Delete(string path)
		{
			GetAdapter().Delete(path);
		}

		public static bool Delete(string path, bool recursively)
		{
			return GetAdapter().Delete(path, recursively);
		}

		public static string GetFullPath(string dir)
		{
			return GetAdapter().GetFullPath(dir);
		}

		public static string MapPath(string path)
		{
			return GetAdapter().MapPath(path);
		}

		public static void Move(string originalPath, string newPath)
		{
			GetAdapter().Move(originalPath, newPath);
		}
	}
}