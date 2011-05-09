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

		public static void Move(string originalPath, string newPath, bool overwrite)
		{
			GetAdapter().Move(originalPath, newPath, overwrite);
		}

		public static bool CreateDirectory(string directoryPath)
		{
			Contract.Requires(!string.IsNullOrEmpty(directoryPath));
			return Create(directoryPath);
		}
	}
}