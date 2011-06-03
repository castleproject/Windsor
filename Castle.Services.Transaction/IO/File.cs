#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Castle.Services.Transaction.IO
{
	/// <summary>
	/// 	Utility class for file operations.
	/// </summary>
	public static class File
	{
		private static IFileAdapter _FileAdapter;

		public static void InitializeWith(IFileAdapter adapter)
		{
			Contract.Requires(adapter != null);
			Contract.Ensures(_FileAdapter != null);

			_FileAdapter = adapter;
		}

		public static void Reset()
		{
			_FileAdapter = null;
		}

		private static IFileAdapter GetAdapter()
		{
			Contract.Requires(_FileAdapter != null);

			if (_FileAdapter == null)
				throw new InvalidOperationException(
					"If you call the Directory API you first need to call Directory.InitializeWith(IDirectoryAdapter)");

			return _FileAdapter;
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

		public static string ReadAllText(this string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().ReadAllText(path);
		}

		public static void WriteAllText(this string path, string contents)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			Contract.Requires(contents != null);
			GetAdapter().WriteAllText(path, contents);
		}

		public static void Delete(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
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
			Contract.Requires(!string.IsNullOrEmpty(newFilePath));
			Contract.Requires(Castle.Services.Transaction.IO.Path.GetFileName(originalFilePath).Length > 0);
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

		public static void WriteAllLines(string path, IEnumerable<string> lines)
		{
			throw new NotImplementedException();
		}
	}
}