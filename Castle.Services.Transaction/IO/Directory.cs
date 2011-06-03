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
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction.IO
{
	/// <summary>
	/// 	Utility class for directories.
	/// </summary>
	public static class Directory
	{
		private static IDirectoryAdapter _DirectoryAdapter;

		public static void InitializeWith(IDirectoryAdapter adapter)
		{
			Contract.Requires(adapter != null);
			Contract.Ensures(_DirectoryAdapter != null);

			if (_DirectoryAdapter != null)
				throw new InvalidOperationException("This method cannot be called twice without resetting the class with Directory.Reset().");

			_DirectoryAdapter = adapter;
		}

		public static void Reset()
		{
			_DirectoryAdapter = null;
		}

		private static IDirectoryAdapter GetAdapter()
		{
			Contract.Requires(_DirectoryAdapter != null);

			if (_DirectoryAdapter == null)
				throw new InvalidOperationException(
					"If you call the Directory API you first need to call Directory.InitializeWith(IDirectoryAdapter)");

			return _DirectoryAdapter;
		}

		public static bool Create(this string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().Create(path);
		}

		/// <summary>
		/// Returns whether the given paths exists.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool Exists(this string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().Exists(path);
		}

		public static void DeleteDirectory(this string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			GetAdapter().Delete(path);
		}

		public static bool DeleteDirectory(this string path, bool recursively)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().Delete(path, recursively);
		}

		public static string GetFullPath(this string dir)
		{
			Contract.Requires(!string.IsNullOrEmpty(dir));
			return GetAdapter().GetFullPath(dir);
		}

		public static string MapPath(this string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			return GetAdapter().MapPath(path);
		}

		public static void Move(this string source, string target)
		{
			Contract.Requires(!string.IsNullOrEmpty(source));
			Contract.Requires(!string.IsNullOrEmpty(target));
			GetAdapter().Move(source, target);
		}

		public static void Move(this string source, string target, bool overwrite)
		{
			GetAdapter().Move(source, target, overwrite);
		}

		public static bool CreateDirectory(this string directoryPath)
		{
			Contract.Requires(!string.IsNullOrEmpty(directoryPath));
			return Create(directoryPath);
		}
	}
}