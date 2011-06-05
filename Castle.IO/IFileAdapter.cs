#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

using System.Diagnostics.Contracts;
using System.IO;
using Castle.IO.Contracts;

namespace Castle.IO
{
	///<summary>
	/// File helper wrapper interface.
	///</summary>
	[ContractClass(typeof(IFileAdapterContract))]
	public interface IFileAdapter
	{
		///<summary>
		/// Creates a new file.
		///</summary>
		///<param name="path">The path, where to create the file.</param>
		///<returns>A handle pointing to the file.</returns>
		FileStream Create(string path);

		///<summary>
		/// Returns whether the specified file exists or not.
		///</summary>
		///<param name="filePath">The file path.</param>
		///<returns></returns>
		bool Exists(string filePath);

		/// <summary>
		/// Delete the file at the given path.
		/// </summary>
		/// <param name="filePath"></param>
		void Delete(string filePath);

		/// <summary>
		/// Opens a file with RW access.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="mode">The file mode, which specifies </param>
		/// <returns></returns>
		FileStream Open(string filePath, FileMode mode);

		FileStream OpenWrite(string path);

		/// <summary>
		/// Creates or opens a file for writing UTF-8 encoded text.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		StreamWriter CreateText(string filePath);

		///<summary>
		/// Moves a file from one path to another.
		/// 
		/// These should all be equivalent:
		/// <code>
		/// Move("b/a.txt", "c/a.txt")
		/// Move("b/a.txt", "c") // given c either is a directory or doesn't exist, otherwise it overwrites the file c
		/// Move("b/a.txt", "c/") // c must be a directory and might or might not exist. If it doesn't exist it will be created.
		/// </code>
		///</summary>
		///<param name="originalFilePath">
		/// The original file path. It can't be null nor can it point to a directory.
		/// </param>
		/// ///<param name="newFilePath">The new location of the file.</param>
		void Move(string originalFilePath, string newFilePath);
	}
}