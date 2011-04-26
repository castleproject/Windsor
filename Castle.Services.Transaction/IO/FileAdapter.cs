#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Services.Transaction.IO
{
	using System;
	using System.IO;
	using System.Text;
	using log4net;

	/// <summary>
	/// Adapter class for the file transactions
	/// which implement the same interface.
	/// 
	/// This adapter chooses intelligently whether there's an ambient
	/// transaction, and if there is, joins it.
	/// </summary>
	public sealed class FileAdapter : TxAdapterBase, IFileAdapter
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof(FileAdapter));

		///<summary>
		/// c'tor
		///</summary>
		public FileAdapter() : this(false, null)
		{
		}

		///<summary>
		/// c'tor
		///</summary>
		///<param name="constrainToSpecifiedDir"></param>
		///<param name="specifiedDir"></param>
		public FileAdapter(bool constrainToSpecifiedDir, string specifiedDir) : base(constrainToSpecifiedDir, specifiedDir)
		{
			if (constrainToSpecifiedDir)
				_Logger.Debug(string.Format("FileAdapter c'tor, constraining to dir: {0}", specifiedDir));
			else
				_Logger.Debug("FileAdapter c'tor, no directory constraint.");
		}

		///<summary>
		/// Creates a new file from the given path for ReadWrite,
		/// different depending on whether we're in a transaction or not.
		///</summary>
		///<param name="path">Path to create file at.</param>
		///<returns>A filestream for the path.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
			Justification = "This is the aim; this is a factory method")]
		public FileStream Create(string path)
		{
			AssertAllowed(path);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
				return (tx as IFileAdapter).Create(path);
#endif
			// TODO: implement using p/invoke
			return System.IO.File.Create(path);
		}

		///<summary>
		/// Returns whether the specified file exists or not.
		///</summary>
		///<param name="filePath">The file path.</param>
		///<returns></returns>
		public bool Exists(string filePath)
		{
			AssertAllowed(filePath);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
				return (tx as IFileAdapter).Exists(filePath);
#endif
			// TODO: implement using p/invoke
			return System.IO.File.Exists(filePath);
		}

		public string ReadAllText(string path, Encoding encoding)
		{
			AssertAllowed(path);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
				return ((IFileAdapter)tx).ReadAllText(path, encoding);
#endif
			// TODO: implement using p/invoke
			return System.IO.File.ReadAllText(path, encoding);
			
		}

		public void Move(string originalFilePath, string newFilePath)
		{
			AssertAllowed(originalFilePath);
			AssertAllowed(newFilePath);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
			{
				((IFileAdapter)tx).Move(originalFilePath, newFilePath);
				return;
			}
#endif
			throw new NotImplementedException();
		}

		public IList<string> ReadAllLines(string filePath)
		{
			throw new NotImplementedException();
		}

		public StreamWriter CreateText(string filePath)
		{
			throw new NotImplementedException();
		}

		public string ReadAllText(string path)
		{
			return ReadAllText(path, Encoding.UTF8);
		}

		public void WriteAllText(string path, string contents)
		{
			AssertAllowed(path);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
			{
				((IFileAdapter)tx).WriteAllText(path, contents);
				return;
			}
#endif
			File.WriteAllText(path, contents);
		}

		public void Delete(string filePath)
		{
			AssertAllowed(filePath);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
			{
				(tx as IFileAdapter).Delete(filePath);
				return;
			}
#endif
			File.Delete(filePath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
			Justification = "This is the intention; it's a factory method")]
		public FileStream Open(string filePath, FileMode mode)
		{
			AssertAllowed(filePath);
#if !MONO
			ITransaction tx;
			if (HasTransaction(out tx))
				return ((IFileAdapter)tx).Open(filePath, mode);
#endif
			return File.Open(filePath, mode);
		}

		public int WriteStream(string toFilePath, Stream fromStream)
		{
			int offset = 0;
			using (var fs = Create(toFilePath))
			{
				var buf = new byte[4096];
				int read;
				while ((read = fromStream.Read(buf, 0, buf.Length)) != 0)
				{
					fs.Write(buf, 0, read);
					offset += read;
				}
			}
			return offset;
		}
	}
}