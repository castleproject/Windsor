using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Castle.Services.Transaction.IO
{
	public static class File
	{
		private static IFileAdapter GetAdapter()
		{
			// TODO: IoC lookup or Something?
			return new FileAdapter();
		}

		public static FileStream Create(string filePath)
		{
			return GetAdapter().Create(filePath);
		}

		public static bool Exists(string filePath)
		{
			return GetAdapter().Exists(filePath);
		}

		public static string ReadAllText(string path)
		{
			return GetAdapter().ReadAllText(path);
		}

		public static void WriteAllText(string path, string contents)
		{
			GetAdapter().WriteAllText(path, contents);
		}

		public static void Delete(string filePath)
		{
			GetAdapter().Delete(filePath);
		}

		public static FileStream Open(string filePath, FileMode mode)
		{
			return GetAdapter().Open(filePath, mode);
		}

		public static int WriteStream(string toFilePath, Stream fromStream)
		{
			return GetAdapter().WriteStream(toFilePath, fromStream);
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			return GetAdapter().ReadAllText(path, encoding);
		}

		public static void Move(string originalFilePath, string newFilePath)
		{
			GetAdapter().Move(originalFilePath, newFilePath);
		}

		public static IList<string> ReadAllLines(string filePath)
		{
			return GetAdapter().ReadAllLines(filePath);
		}

		public static StreamWriter CreateText(string filePath)
		{
			return GetAdapter().CreateText(filePath);
		}
	}
}