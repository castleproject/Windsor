#region License

//  Copyright 2004-2010 Henrik Feldt, Castle Project - http://www.castleproject.org/
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Transactions;
using Castle.Services.Transaction.IO;
using Castle.Services.vNextTransaction;
using Microsoft.Win32.SafeHandles;
using Path = Castle.Services.Transaction.IO.Path;
using TransactionException = System.Transactions.TransactionException;

namespace Castle.Services.Transaction
{
	///<summary>
	/// Represents a transaction on transactional kernels
	/// like the Vista kernel or Server 2008 kernel and newer.
	///</summary>
	/// <remarks>
	/// Good information for dealing with the peculiarities of the runtime:
	/// http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.aspx
	/// </remarks>
	internal sealed class FileTransaction : /* Tranasction, */ IFileAdapter, IDirectoryAdapter, ITransaction
	{
		private readonly string _Name;
		private SafeKernelTxHandle _TransactionHandle;
		private bool _Disposed;

		#region Constructors

		///<summary>
		/// c'tor w/o name.
		///</summary>
		public FileTransaction() : this(null)
		{
		}

		///<summary>
		/// c'tor for the file transaction.
		///</summary>
		///<param name="name">The name of the transaction.</param>
		public FileTransaction(string name)
		{
			_Name = name;
		}

		#endregion

		#region ITransaction Members

		///<summary>
		/// Gets the name of the transaction.
		///</summary>
		public string Name
		{
			get { return _Name ?? string.Format("TxF #{0}", GetHashCode()); }
		}

		protected void InnerBegin()
		{
			// we have a ongoing current transaction, join it!
			if (System.Transactions.Transaction.Current != null)
			{
				var ktx = (IKernelTransaction) TransactionInterop.GetDtcTransaction(System.Transactions.Transaction.Current);

				SafeKernelTxHandle handle;
				ktx.GetHandle(out handle);

				// even though _TransactionHandle can already contain a handle if this thread 
				// had been yielded just before setting this reference, the "safe"-ness of the wrapper should
				// not dispose the other handle which is now removed
				_TransactionHandle = handle;
				//IsAmbient = true; // TODO: Perhaps we created this item and we need to notify the transaction manager...
			}
			else _TransactionHandle = NativeMethods.createTransaction(string.Format("{0} Transaction", _Name));
			if (!_TransactionHandle.IsInvalid) return;

			throw new TransactionException(
				"Cannot begin file transaction. CreateTransaction failed and there's no ambient transaction.",
				LastEx());
		}

		private Exception LastEx()
		{
			return Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
		}

		protected void InnerCommit()
		{
			if (NativeMethods.CommitTransaction(_TransactionHandle)) return;
			throw new TransactionException("Commit failed.", LastEx());
		}

		protected void InnerRollback()
		{
			if (!NativeMethods.RollbackTransaction(_TransactionHandle))
				throw new TransactionException("Rollback failed.",
				                               Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
		}

		#endregion

		#region IFileAdapter & IDirectoryAdapter >> Ambiguous members

		FileStream IFileAdapter.Create(string path)
		{
			Contract.Requires(path != null);
			AssertState(TransactionStatus.Active);

			return open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		/// <summary>Creates a directory at the path given.</summary>
		///<param name="path">The path to create the directory at.</param>
		bool IDirectoryAdapter.Create(string path)
		{
			Contract.Requires(path != null);
			AssertState(TransactionStatus.Active);

			path = Path.NormDirSepChars(CleanPathEnd(path));

			// we don't need to re-create existing folders.
			if (((IDirectoryAdapter) this).Exists(path))
				return true;

			var nonExistent = new Stack<string>();
			nonExistent.Push(path);

			string curr = path;
			while (!((IDirectoryAdapter) this).Exists(curr)
			       && (curr.Contains(System.IO.Path.DirectorySeparatorChar)
			           || curr.Contains(System.IO.Path.AltDirectorySeparatorChar)))
			{
				curr = Path.GetPathWithoutLastBit(curr);

				if (!((IDirectoryAdapter) this).Exists(curr))
					nonExistent.Push(curr);
			}

			while (nonExistent.Count > 0)
			{
				if (!createDirectoryTransacted(nonExistent.Pop()))
				{
					var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
					throw new TransactionException(string.Format("Failed to create directory \"{1}\" at path \"{0}\". "
					                                             + "See inner exception for more details.", path, curr),
					                               win32Exception);
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the current transaction status.
		/// </summary>
		public TransactionStatus Status { get; private set; }

		protected void AssertState(TransactionStatus status)
		{
			AssertState(status, null);
		}

		protected void AssertState(TransactionStatus status, string msg)
		{
			if (status != Status)
			{
				if (!string.IsNullOrEmpty(msg))
					throw new TransactionException(msg);

				throw new TransactionException(string.Format("State failure; should have been {0} but was {1}",
															 status, Status));
			}
		}


		/// <summary>
		/// Deletes a file as part of a transaction
		/// </summary>
		/// <param name="filePath"></param>
		void IFileAdapter.Delete(string filePath)
		{
			Contract.Requires(filePath != null);
			AssertState(TransactionStatus.Active);

			if (!NativeMethods.DeleteFileTransactedW(filePath, _TransactionHandle))
			{
				var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
				throw new TransactionException("Unable to perform transacted file delete.", win32Exception);
			}
		}

		/// <summary>
		/// Deletes a folder recursively.
		/// </summary>
		/// <param name="path">The directory path to start deleting at!</param>
		void IDirectoryAdapter.Delete(string path)
		{
			Contract.Requires(path != null);
			AssertState(TransactionStatus.Active);

			if (!NativeMethods.RemoveDirectoryTransactedW(path, _TransactionHandle))
				throw new TransactionException("Unable to delete folder. See inner exception for details.",
				                               new Win32Exception(Marshal.GetLastWin32Error()));
		}

		bool IFileAdapter.Exists(string filePath)
		{
			Contract.Requires(filePath != null);
			AssertState(TransactionStatus.Active);

			using (var handle = NativeMethods.FindFirstFileTransacted(filePath, false, _TransactionHandle))
				return !handle.IsInvalid;
		}

		/// <summary>
		/// Checks whether the path exists.
		/// </summary>
		/// <param name="path">Path to check.</param>
		/// <returns>True if it exists, false otherwise.</returns>
		bool IDirectoryAdapter.Exists(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			AssertState(TransactionStatus.Active);

			path = CleanPathEnd(path);

			using (var handle = NativeMethods.FindFirstFileTransacted(path, true, _TransactionHandle))
				return !handle.IsInvalid;
		}

		string IDirectoryAdapter.GetFullPath(string dir)
		{
			Contract.Requires(dir != null);
			AssertState(TransactionStatus.Active);
			return getFullPathNameTransacted(dir);
		}

		string IDirectoryAdapter.MapPath(string path)
		{
			throw new NotSupportedException("Implemented on the directory adapter.");
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath)
		{
			Contract.Requires(originalPath != null);
			Contract.Requires(newPath != null);

			var da = ((IDirectoryAdapter) this);

			if (!da.Exists(originalPath))
				throw new DirectoryNotFoundException(
					string.Format("The path \"{0}\" could not be found. The source directory needs to exist.",
					              originalPath));

			if (!da.Exists(newPath))
				da.Create(newPath);

			if (!NativeMethods.MoveFileTransacted(originalPath, newPath, IntPtr.Zero, IntPtr.Zero,
											 NativeMethods.MoveFileFlags.ReplaceExisting, _TransactionHandle))
				throw new TransactionException("Could not move directory", LastEx());

			// TODO: Complete.
			recurseFiles(originalPath,
			f =>
			{
			    Console.WriteLine("file: {0}", f);
			    return true;
			},
			d =>
			{
			    Console.WriteLine("dir: {0}", d);
			    return true;
			});
		}

		IList<string> IFileAdapter.ReadAllLines(string filePath)
		{
			throw new NotImplementedException();
		}

		public StreamWriter CreateText(string filePath)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IFileAdapter members

		/// <summary>
		/// Opens a file with RW access.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="mode">The file mode, which specifies </param>
		/// <returns></returns>
		public FileStream Open(string filePath, FileMode mode)
		{
			Contract.Requires(filePath != null);

			return open(filePath, mode, FileAccess.ReadWrite, FileShare.None);
		}

		/// <summary>
		/// DO NOT USE: Implemented in the file adapter: <see cref="FileAdapter"/>.
		/// </summary>
		int IFileAdapter.WriteStream(string toFilePath, Stream fromStream)
		{
			throw new NotSupportedException("Use the file adapter instead!!");
		}

		///<summary>
		/// Reads all text in a file and returns the string of it.
		///</summary>
		///<param name="path"></param>
		///<param name="encoding"></param>
		///<returns></returns>
		public string ReadAllText(string path, Encoding encoding)
		{
			AssertState(TransactionStatus.Active);

			using (var reader = new StreamReader(open(path, FileMode.Open, FileAccess.Read, FileShare.Read), encoding))
			{
				return reader.ReadToEnd();
			}
		}

		void IFileAdapter.Move(string originalFilePath, string newFilePath)
		{
			// case 1, the new file path is a folder
			if (((IDirectoryAdapter) this).Exists(newFilePath))
			{
				NativeMethods.MoveFileTransacted(originalFilePath, newFilePath.Combine(Path.GetFileName(originalFilePath)), IntPtr.Zero,
				                   IntPtr.Zero, NativeMethods.MoveFileFlags.CopyAllowed,
				                   _TransactionHandle);
				return;
			}

			// case 2, its not a folder, so assume it's a file.
			NativeMethods.MoveFileTransacted(originalFilePath, newFilePath, IntPtr.Zero, IntPtr.Zero, NativeMethods.MoveFileFlags.CopyAllowed,
			                   _TransactionHandle);
		}

		/// <summary>
		/// Reads all text from a file as part of a transaction
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ReadAllText(string path)
		{
			AssertState(TransactionStatus.Active);

			using (var reader = new StreamReader(open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Writes text to a file as part of a transaction.
		/// If the file already contains data, first truncates the file
		/// and then writes all contents in the string to the file.
		/// </summary>
		/// <param name="path">Path to write to</param>
		/// <param name="contents">Contents of the file after writing to it.</param>
		public void WriteAllText(string path, string contents)
		{
			AssertState(TransactionStatus.Active);

			bool exists = ((IFileAdapter) this).Exists(path);
			using (
				var writer =
					new StreamWriter(open(path, exists ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
			{
				writer.Write(contents);
			}
		}

		#endregion

		#region IDirectoryAdapter members

		/// <summary>
		/// Deletes an empty directory
		/// </summary>
		/// <param name="path">The path to the folder to delete.</param>
		/// <param name="recursively">
		/// Whether to delete recursively or not.
		/// When recursive, we delete all subfolders and files in the given
		/// directory as well.
		/// </param>
		bool IDirectoryAdapter.Delete(string path, bool recursively)
		{
			AssertState(TransactionStatus.Active);
			return recursively
			       	? deleteRecursive(path)
					: NativeMethods.RemoveDirectoryTransactedW(path, _TransactionHandle);
		}

		#endregion

		#region Dispose-pattern

		///<summary>
		/// Gets whether the transaction is disposed.
		///</summary>
		public bool IsDisposed
		{
			get { return _Disposed; }
		}

		/// <summary>
		/// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other 
		/// cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
		/// </summary>
		~FileTransaction()
		{
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public /*override*/ void Dispose()
		{
			Dispose(true);

			// the base transaction dispose all resources active, so we must be careful
			// and call our own resources first, thereby having to call this afterwards.
			//base.Dispose();

			GC.SuppressFinalize(this);
		}

		TransactionState ITransaction.State
		{
			get { throw new NotImplementedException(); }
		}

		ITransactionOptions ITransaction.CreationOptions
		{
			get { throw new NotImplementedException(); }
		}

		System.Transactions.Transaction ITransaction.Inner
		{
			get { throw new NotImplementedException(); }
		}

		public Maybe<SafeKernelTxHandle> TxFHandle
		{
			get { throw new NotImplementedException(); }
		}

		Maybe<IRetryPolicy> ITransaction.FailedPolicy
		{
			get { throw new NotImplementedException(); }
		}

		string ITransaction.LocalIdentifier
		{
			get { throw new NotImplementedException(); }
		}

		void ITransaction.Rollback()
		{
			throw new NotImplementedException();
		}

		void ITransaction.Complete()
		{
			throw new NotImplementedException();
		}

		[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
		private void Dispose(bool disposing)
		{
			// no unmanaged code here, just return.
			if (!disposing) return;

			if (_Disposed) return;
			// called via the Dispose() method on IDisposable, 
			// can use private object references.

			if (Status == TransactionStatus.Active)
				//Rollback();
				throw new NotImplementedException();

			if (_TransactionHandle != null && !_TransactionHandle.IsInvalid)
				_TransactionHandle.Dispose();

			_Disposed = true;
		}

		#endregion

		#region C++ Interop

		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedMember.Local

		// overview here: http://msdn.microsoft.com/en-us/library/aa964885(VS.85).aspx
		// helper: http://www.improve.dk/blog/2009/02/14/utilizing-transactional-ntfs-through-dotnet

		#region Helper methods

		private const int ERROR_TRANSACTIONAL_CONFLICT = 0x1A90;

		/// <summary>
		/// Creates a file handle with the current ongoing transaction.
		/// </summary>
		/// <param name="path">The path of the file.</param>
		/// <param name="mode">The file mode, i.e. what is going to be done if it exists etc.</param>
		/// <param name="access">The access rights this handle has.</param>
		/// <param name="share">What other handles may be opened; sharing settings.</param>
		/// <returns>A safe file handle. Not null, but may be invalid.</returns>
		private FileStream open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			// Future: Support System.IO.FileOptions which is the dwFlagsAndAttribute parameter.
			SafeFileHandle fileHandle = NativeMethods.CreateFileTransactedW(path,
			                                                  translateFileAccess(access),
			                                                  translateFileShare(share),
			                                                  IntPtr.Zero,
			                                                  translateFileMode(mode),
			                                                  0, IntPtr.Zero,
			                                                  _TransactionHandle,
			                                                  IntPtr.Zero, IntPtr.Zero);

			if (fileHandle.IsInvalid)
			{
				int error = Marshal.GetLastWin32Error();
				string baseStr = string.Format("Transaction \"{1}\": Unable to open a file descriptor to \"{0}\".", path,
				                               Name ?? "[no name]");

				if (error == ERROR_TRANSACTIONAL_CONFLICT)
					throw new TransactionalConflictException(baseStr
					                                         + " You will get this error if you are accessing the transacted file from a non-transacted API before the transaction has "
					                                         + "committed. See HelperLink for details.",
															 new Uri("http://msdn.microsoft.com/en-us/library/aa365536%28VS.85%29.aspx"));

				throw new TransactionException(baseStr + "Please see the inner exceptions for details.",
				                               new Win32Exception(Marshal.GetLastWin32Error()));
			}

			return new FileStream(fileHandle, access);
		}

		/// <summary>
		/// Managed -> Native mapping
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		private static NativeMethods.NativeFileMode translateFileMode(FileMode mode)
		{
			if (mode != FileMode.Append)
				return (NativeMethods.NativeFileMode) (uint) mode;
			return (NativeMethods.NativeFileMode) (uint) FileMode.OpenOrCreate;
		}

		/// <summary>
		/// Managed -> Native mapping
		/// </summary>
		/// <param name="access"></param>
		/// <returns></returns>
		private static NativeMethods.NativeFileAccess translateFileAccess(FileAccess access)
		{
			switch (access)
			{
				case FileAccess.Read:
					return NativeMethods.NativeFileAccess.GenericRead;
				case FileAccess.Write:
					return NativeMethods.NativeFileAccess.GenericWrite;
				case FileAccess.ReadWrite:
					return NativeMethods.NativeFileAccess.GenericRead | NativeMethods.NativeFileAccess.GenericWrite;
				default:
					throw new ArgumentOutOfRangeException("access");
			}
		}

		/// <summary>
		/// Direct Managed -> Native mapping
		/// </summary>
		/// <param name="share"></param>
		/// <returns></returns>
		private static NativeMethods.NativeFileShare translateFileShare(FileShare share)
		{
			return (NativeMethods.NativeFileShare) (uint) share;
		}

		private bool createDirectoryTransacted(string templatePath,
		                                       string dirPath)
		{
			return NativeMethods.CreateDirectoryTransactedW(templatePath,
			                                  dirPath,
			                                  IntPtr.Zero,
			                                  _TransactionHandle);
		}

		private bool createDirectoryTransacted(string dirPath)
		{
			return createDirectoryTransacted(null, dirPath);
		}

		private bool deleteRecursive(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			return recurseFiles(path,
								file => NativeMethods.DeleteFileTransactedW(file, _TransactionHandle),
								dir => NativeMethods.RemoveDirectoryTransactedW(dir, _TransactionHandle));
		}

		private bool recurseFiles(string path,
		                          Func<string, bool> operationOnFiles,
		                          Func<string, bool> operationOnDirectories)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			NativeMethods.WIN32_FIND_DATA findData;
			bool addPrefix = !path.StartsWith(@"\\?\");
			bool ok = true;

			string pathWithoutSufflix = addPrefix ? @"\\?\" + Path.GetFullPath(path) : Path.GetFullPath(path);
			path = pathWithoutSufflix + "\\*";

			using (var findHandle = NativeMethods.FindFirstFileTransactedW(path, _TransactionHandle, out findData))
			{
				if (findHandle.IsInvalid) return false;

				do
				{
					string subPath = pathWithoutSufflix.Combine(findData.cFileName);

					if ((findData.dwFileAttributes & (uint) FileAttributes.Directory) != 0)
					{
						if (findData.cFileName != "." && findData.cFileName != "..")
							ok &= deleteRecursive(subPath);
					}
					else
						ok = ok && operationOnFiles(subPath);
				} while (NativeMethods.FindNextFile(findHandle, out findData));
			}

			return ok && operationOnDirectories(pathWithoutSufflix);
		}

		/*
		 * Might need to use:
		 * DWORD WINAPI GetLongPathNameTransacted(
		 *	  __in   LPCTSTR lpszShortPath,
		 *	  __out  LPTSTR lpszLongPath,
		 *	  __in   DWORD cchBuffer,
		 *	  __in   HANDLE hTransaction
		 *	);
		 */

		private string getFullPathNameTransacted(string dirOrFilePath)
		{
			var sb = new StringBuilder(512);

			retry:
			IntPtr p = IntPtr.Zero;
			int res = NativeMethods.GetFullPathNameTransactedW(dirOrFilePath,
			                                     sb.Capacity,
			                                     sb,
			                                     ref p, // here we can check if it's a file or not.
			                                     _TransactionHandle);

			if (res == 0) // failure
			{
				throw new TransactionException(
					string.Format("Could not get full path for \"{0}\", see inner exception for details.",
					              dirOrFilePath),
					Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
			}
			if (res > sb.Capacity)
			{
				sb.Capacity = res; // update capacity
				goto retry; // handle edge case if the path.Length > 512.
			}

			return sb.ToString();
		}

		// more examples in C++:  
		// http://msdn.microsoft.com/en-us/library/aa364963(VS.85).aspx
		// http://msdn.microsoft.com/en-us/library/x3txb6xc.aspx

		#endregion

		// ReSharper restore UnusedMember.Local
		// ReSharper restore InconsistentNaming
#pragma warning restore 1591

		#endregion

		#region Minimal utils

		private static string CleanPathEnd(string path)
		{
			return path.TrimEnd('/', '\\');
		}

		#endregion
	}
}