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

namespace Castle.IO
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Extensions for <see cref="IFile"/>
	/// </summary>
	public static class FileExtensions
	{
		public static Stream OpenRead(this IFile file)
		{
			Contract.Requires(file != null);
			Contract.Ensures(Contract.Result<Stream>() != null);

			return file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public static Stream OpenAppend(this IFile file)
		{
			Contract.Requires(file != null);
			Contract.Ensures(Contract.Result<Stream>() != null);

			return file.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
		}

		public static Stream OpenWrite(this IFile file)
		{
			Contract.Requires(file != null);
			Contract.Ensures(Contract.Result<Stream>() != null);

			return file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		/// <summary>
		/// </summary>
		/// <param name = "file">Invokee, the file that is to be fully read.</param>
		/// <returns>All of the file contents as a string. If the current position is at the end of the stream, returns the empty string("").</returns>
		/// <exception cref = "OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
		/// <exception cref = "IOException">An IO error occrs</exception>
		public static string ReadAllText(this IFile file)
		{
			Contract.Requires(file != null);
			Contract.Ensures(Contract.Result<string>() != null);

			using (var sr = new StreamReader(file.OpenRead()))
				return sr.ReadToEnd();
		}

		public static void WriteAllText(this IFile file, string contents)
		{
			Contract.Requires(file != null);
			Contract.Requires(contents != null, "content't mustn't be null, but it may be empty");

			using (var sw = new StreamWriter(OpenWrite(file)))
				sw.Write(contents);
		}

		/// <exception cref="NotImplementedException"></exception>
		public static int WriteStream(string targetPath, Stream sourceStream)
		{
			Contract.Requires(!string.IsNullOrEmpty(targetPath));

			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException"></exception>
		public static string ReadAllText(string path, Encoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException"></exception>
		public static IEnumerable<string> ReadAllLines(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));

			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException"></exception>
		public static IEnumerable<string> ReadAllLinesEnumerable(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));

			throw new NotImplementedException();
		}
	}
}