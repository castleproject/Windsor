#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Text;

namespace Castle.IO
{
	public static class FileExtensions
	{
		public static T MustExist<T>(this T directory) where T : IFileSystemItem<T>
		{
			if (!directory.Exists)
				directory.Create();
			return directory;
		}

		public static IDirectory FindDirectory(this IDirectory directory, string path)
		{
			var child = directory.GetDirectory(path);
			return child.Exists ? child : null;
		}

		public static IFile FindFile(this IDirectory directory, string filename)
		{
			var child = directory.GetFile(filename);
			return child.Exists ? child : null;
		}

		public static IDirectory GetOrCreateDirectory(this IDirectory directory, params string[] childDirectories)
		{
			return childDirectories.Aggregate(directory,
			                                  (current, childDirectoryName) =>
			                                  MustExist(current.GetDirectory(childDirectoryName)));
		}

		public static Stream OpenRead(this IFile file)
		{
			return file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public static Stream OpenAppend(this IFile file)
		{
			return file.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
		}

		public static Stream OpenWrite(this IFile file)
		{
			return file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		public static IEnumerable<IDirectory> AncestorsAndSelf(this IDirectory directory)
		{
			do
			{
				yield return directory;
				directory = directory.Parent;
			} while (directory != null);
		}

		public static string ReadAllText(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		public static void WriteAllText(string targetPath, string contents)
		{
			Contract.Requires(!string.IsNullOrEmpty(targetPath));
			Contract.Requires(contents != null, "content't mustn't be null, but it may be empty");
			throw new NotImplementedException();
		}

		public static int WriteStream(string targetPath, Stream sourceStream)
		{
			Contract.Requires(!string.IsNullOrEmpty(targetPath));
			throw new NotImplementedException();
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		public static IEnumerable<string> ReadAllLines(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		public static IEnumerable<string> ReadAllLinesEnumerable(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

	}
}