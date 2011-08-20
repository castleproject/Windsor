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

namespace Castle.IO
{
	public interface IFileSystemItem<out T> : IFileSystemItem
		where T : IFileSystemItem
	{
		T Create();
	}

	public interface IFileSystemItem
	{
		Path Path { get; }

		IDirectory Parent { get; }

		IFileSystem FileSystem { get; }

		bool Exists { get; }

		string Name { get; }

		/// <summary>
		/// 	Deletes the item from the file system.
		/// </summary>
		void Delete();

		/// <summary>
		/// 	Copies the callee to the file system item passed as parameter,
		/// 	and overwrites it if it already exists.
		/// </summary>
		/// <param name = "item">The target of the copy. Targets that work:
		/// 	<list>
		/// 		<item>Directory -> Directory, OK</item>
		/// 		<item>Directory -> File, Exception</item>
		/// 		<item>File -> File, OK</item>
		/// 		<item>File -> Directory, OK</item>
		/// 	</list>
		/// </param>
		void CopyTo(IFileSystemItem item);

		void MoveTo(IFileSystemItem item);
	}
}