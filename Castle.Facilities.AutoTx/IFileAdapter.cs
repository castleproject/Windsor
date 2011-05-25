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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using Castle.Facilities.Transactions.Contracts;

namespace Castle.Facilities.Transactions
{
	///<summary>
	/// File helper wrapper interface.
	///</summary>
	[ContractClass(typeof(IFileAdapterContract))]
	public interface IFileAdapter
	{
		///<summary>
		/// Create a new file transactionally.
		///</summary>
		///<param name="filePath">The path, where to create the file.</param>
		///<returns>A handle pointing to the file.</returns>
		FileStream Create(string filePath);

		///<summary>
		/// Returns whether the specified file exists or not.
		///</summary>
		///<param name="filePath">The file path.</param>
		///<returns></returns>
		bool Exists(string filePath);

		/// <summary>
		/// Reads all text from a file as part of a transaction
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		string ReadAllText(string path);

		/// <summary>
		/// Writes text to a file as part of a transaction.
		/// If the file already contains data, first truncates the file
		/// and then writes all contents in the string to the file.
		/// </summary>
		/// <param name="path">Path to write to</param>
		/// <param name="contents">Contents of the file after writing to it.</param>
		void WriteAllText(string path, string contents);

		/// <summary>
		/// Deletes a file as part of a transaction
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

		/// <summary>
		/// Writes an input stream to the file path.
		/// </summary>
		/// <param name="toFilePath">The path to write to.</param>
		/// <param name="fromStream">The stream to read from.</param>
		/// <returns>The number of bytes written.</returns>
		int WriteStream(string toFilePath, Stream fromStream);

		///<summary>
		/// Reads all text in a file and returns the string of it.
		///</summary>
		///<param name="path"></param>
		///<param name="encoding"></param>
		///<returns></returns>
		string ReadAllText(string path, Encoding encoding);

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

		/// <summary>
		/// Read all lines in the given path.
		/// </summary>
		/// <param name="filePath"></param>
		IList<string> ReadAllLines(string filePath);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		StreamWriter CreateText(string filePath);

		IEnumerable<string> ReadAllLinesEnumerable(string filePath);
	}
}