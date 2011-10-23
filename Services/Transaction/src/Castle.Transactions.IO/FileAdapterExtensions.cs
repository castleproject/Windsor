using System.Collections.Generic;
using System.IO;
using System.Text;
using Castle.IO;

namespace Castle.Transactions.IO
{
	public static class FileAdapterExtensions
	{
		/// <summary>
		/// Writes an input stream to the file path.
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="targetPath">The path to write to.</param>
		/// <param name="sourceStream">The stream to read from.</param>
		/// <returns>The number of bytes written.</returns>
		public static int WriteStream(this IFileAdapter adapter, string targetPath, Stream sourceStream)
		{
			return -1;
		}

		/// <summary>
		/// Writes text to a file as part of a transaction.
		/// If the file already contains data, first truncates the file
		/// and then writes all contents in the string to the file.
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="targetPath">Path to write to</param>
		/// <param name="contents">Contents of the file after writing to it.</param>
		public static void WriteAllText(this IFileAdapter adapter, string targetPath, string contents)
		{
		}

		public static void WriteAllLines(this IFileAdapter a, string path, params string[] lines)
		{
		}
		public static void WriteAllLines(this IFileAdapter a, string path, IEnumerable<string> lines)
		{
		}

		/// <summary>
		/// Reads all text from a file as part of a transaction
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public static string ReadAllText(this IFileAdapter adapter, string source)
		{
			return "";
		}

		///<summary>
		/// Reads all text in a file and returns the string of it.
		///</summary>
		///<param name="adapter"></param>
		///<param name="path"></param>
		///<param name="encoding"></param>
		///<returns></returns>
		public static string ReadAllText(this IFileAdapter adapter, string path, Encoding encoding)
		{
			return "";
		}

		/// <summary>
		/// Read all lines in the given path.
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="filePath"></param>
		public static IEnumerable<string> ReadAllLines(this IFileAdapter adapter, string filePath)
		{
			return new[] {""};
		}

	}
}