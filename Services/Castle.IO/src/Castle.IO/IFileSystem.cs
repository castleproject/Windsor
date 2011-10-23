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
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Interface denoting a file system. This is the core abstraction of Castle IO; providing 
	/// file systems that work across platforms.
	/// </summary>
	[ContractClass(typeof(IFileSystem))]
	public interface IFileSystem
	{
		/// <summary>
		/// Creates a new dictory pointer given a directory path. This path may be relative, absolute or UNC.
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <returns></returns>
		IDirectory GetDirectory(string directoryPath);
		
		/// <summary>
		/// Gets 
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <returns></returns>
		IDirectory GetDirectory(Path directoryPath);
		
		Path GetPath(string path);
		
		ITemporaryDirectory CreateTempDirectory();
		
		IDirectory CreateDirectory(string path);
		
		IDirectory CreateDirectory(Path path);
		
		IFile GetFile(string itemSpec);
		
		ITemporaryFile CreateTempFile();
		
		IDirectory GetTempDirectory();
		
		IDirectory GetCurrentDirectory();
	}
}