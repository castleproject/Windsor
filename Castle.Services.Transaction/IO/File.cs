using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Castle.Services.Transaction.IO
{
	/// <summary>
	/// Utility class for file operations.
	/// </summary>
	public static class File
	{
		private static IFileAdapter GetAdapter()
		{
			// TODO: IoC lookup or Something?
			return new FileAdapter();
		}

		public static FileStream Create(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			return GetAdapter().Create(filePath);
		}

		public static bool Exists(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			return GetAdapter().Exists(filePath);
		}

		public static string ReadAllText(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().ReadAllText(path);
		}

		public static void WriteAllText(string path, string contents)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			GetAdapter().WriteAllText(path, contents);
		}

		public static void Delete(string filePath)
		{
			GetAdapter().Delete(filePath);
		}

		public static FileStream Open(string filePath, FileMode mode)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			return GetAdapter().Open(filePath, mode);
		}

		public static int WriteStream(string toFilePath, Stream fromStream)
		{
			Contract.Requires(!string.IsNullOrEmpty(toFilePath));
			return GetAdapter().WriteStream(toFilePath, fromStream);
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().ReadAllText(path, encoding);
		}

		public static void Move(string originalFilePath, string newFilePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalFilePath));
			GetAdapter().Move(originalFilePath, newFilePath);
		}

		public static IList<string> ReadAllLines(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			return GetAdapter().ReadAllLines(filePath);
		}

		public static StreamWriter CreateText(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			return GetAdapter().CreateText(filePath);
		}
	}
}