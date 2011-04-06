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

namespace Castle.Services.Transaction
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Permissions;
	using System.Text;
	using System.Transactions;
	using IO;
	using Microsoft.Win32.SafeHandles;
	using Path = IO.Path;

	///<summary>
	/// Represents a transaction on transactional kernels
	/// like the Vista kernel or Server 2008 kernel and newer.
	///</summary>
	/// <remarks>
	/// Good information for dealing with the peculiarities of the runtime:
	/// http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.aspx
	/// </remarks>
	public sealed class FileTransaction : TransactionBase, IFileTransaction
	{
		private SafeTxHandle _TransactionHandle;
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
			: base(name, TransactionMode.Unspecified, IsolationMode.ReadCommitted)
		{
		}

		#endregion

		#region ITransaction Members

		// This isn't really relevant with the current architecture

		/// <summary>
		/// Gets whether the transaction is a distributed transaction.
		/// </summary>
		public override bool IsAmbient { get; protected set; }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

		///<summary>
		/// Gets the name of the transaction.
		///</summary>
		public override string Name
		{
			get { return theName ?? string.Format("FtX #{0}", GetHashCode()); }
		}

		protected override void InnerBegin()
		{
		retry:
			// we have a ongoing current transaction, join it!
			if (System.Transactions.Transaction.Current != null)
			{
				var ktx = TransactionInterop.GetDtcTransaction(System.Transactions.Transaction.Current)
						  as IKernelTransaction;

				// check for race-condition.
				if (ktx == null)
					goto retry;

				SafeTxHandle handle;
				ktx.GetHandle(out handle);

				// even though _TransactionHandle can already contain a handle if this thread 
				// had been yielded just before setting this reference, the "safe"-ness of the wrapper should
				// not dispose the other handle which is now removed
				_TransactionHandle = handle; // see the link in the remarks to the class for more details about async exceptions
				IsAmbient = true;
			}
			else _TransactionHandle = createTransaction(string.Format("{0} Transaction", theName));
			if (!_TransactionHandle.IsInvalid) return;

			throw new TransactionException(
				"Cannot begin file transaction because we got a null pointer back from CreateTransaction.",
				LastEx());
		}

		private Exception LastEx()
		{
			return Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
		}

		protected override void InnerCommit()
		{
			if (CommitTransaction(_TransactionHandle)) return;
			throw new TransactionException("Commit failed.", LastEx());
		}

		protected override void InnerRollback()
		{
			if (!RollbackTransaction(_TransactionHandle))
				throw new TransactionException("Rollback failed.",
											   Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
		}

		#endregion

		#region IFileAdapter & IDirectoryAdapter >> Ambiguous members

		FileStream IFileAdapter.Create(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			AssertState(TransactionStatus.Active);

			return open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		/// <summary>Creates a directory at the path given.</summary>
		///<param name="path">The path to create the directory at.</param>
		bool IDirectoryAdapter.Create(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			AssertState(TransactionStatus.Active);

			path = Path.NormDirSepChars(CleanPathEnd(path));

			// we don't need to re-create existing folders.
			if (((IDirectoryAdapter)this).Exists(path))
				return true;

			var nonExistent = new Stack<string>();
			nonExistent.Push(path);

			var curr = path;
			while (!((IDirectoryAdapter)this).Exists(curr)
			       && (curr.Contains(System.IO.Path.DirectorySeparatorChar)
			           || curr.Contains(System.IO.Path.AltDirectorySeparatorChar)))
			{
				curr = Path.GetPathWithoutLastBit(curr);

				if (!((IDirectoryAdapter)this).Exists(curr))
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
		/// Deletes a file as part of a transaction
		/// </summary>
		/// <param name="filePath"></param>
		void IFileAdapter.Delete(string filePath)
		{
			if (filePath == null) throw new ArgumentNullException("filePath");
			AssertState(TransactionStatus.Active);

			if (!DeleteFileTransactedW(filePath, _TransactionHandle))
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
			if (path == null) throw new ArgumentNullException("path");
			AssertState(TransactionStatus.Active);

			if (!RemoveDirectoryTransactedW(path, _TransactionHandle))
				throw new TransactionException("Unable to delete folder. See inner exception for details.",
				                               new Win32Exception(Marshal.GetLastWin32Error()));
		}

		bool IFileAdapter.Exists(string filePath)
		{
			if (filePath == null) throw new ArgumentNullException("filePath");
			AssertState(TransactionStatus.Active);

			using (var handle = findFirstFileTransacted(filePath, false))
				return !handle.IsInvalid;
		}

		/// <summary>
		/// Checks whether the path exists.
		/// </summary>
		/// <param name="path">Path to check.</param>
		/// <returns>True if it exists, false otherwise.</returns>
		bool IDirectoryAdapter.Exists(string path)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
			AssertState(TransactionStatus.Active);

			path = CleanPathEnd(path);

			using (var handle = findFirstFileTransacted(path, true))
				return !handle.IsInvalid;
		}

		string IDirectoryAdapter.GetFullPath(string dir)
		{
			if (dir == null) throw new ArgumentNullException("dir");
			AssertState(TransactionStatus.Active);
			return getFullPathNameTransacted(dir);
		}

		string IDirectoryAdapter.MapPath(string path)
		{
			throw new NotSupportedException("Implemented on the directory adapter.");
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath)
		{
			if (originalPath == null) throw new ArgumentNullException("originalPath");
			if (newPath == null) throw new ArgumentNullException("newPath");

			var da = ((IDirectoryAdapter)this);

			if (!da.Exists(originalPath))
				throw new DirectoryNotFoundException(string.Format("The path \"{0}\" could not be found. The source directory needs to exist.", 
				                                                   originalPath));

			if (!da.Exists(newPath))
				da.Create(newPath);

			// TODO: Complete.
			recurseFiles(originalPath,
			             f => { Console.WriteLine("file: {0}", f); return true; },
			             d => { Console.WriteLine("dir: {0}", d); return true; });
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
			if (filePath == null) throw new ArgumentNullException("filePath");

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
			if (((IDirectoryAdapter)this).Exists(newFilePath))
			{
				MoveFileTransacted(originalFilePath, newFilePath.Combine(Path.GetFileName(originalFilePath)), IntPtr.Zero, IntPtr.Zero, MoveFileFlags.CopyAllowed,
				                   _TransactionHandle);
				return;
			}

			// case 2, its not a folder, so assume it's a file.
			MoveFileTransacted(originalFilePath, newFilePath, IntPtr.Zero, IntPtr.Zero, MoveFileFlags.CopyAllowed,
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

			var exists = ((IFileAdapter) this).Exists(path);
			using (var writer = new StreamWriter(open(path, exists ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
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
			return recursively ? deleteRecursive(path) 
			       	: RemoveDirectoryTransactedW(path, _TransactionHandle);
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
		public override void Dispose()
		{
			Dispose(true);
			
			// the base transaction dispose all resources active, so we must be careful
			// and call our own resources first, thereby having to call this afterwards.
			base.Dispose();

			GC.SuppressFinalize(this);
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
				Rollback();

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
			var fileHandle = CreateFileTransactedW(path,
			                                       translateFileAccess(access),
			                                       translateFileShare(share),
			                                       IntPtr.Zero,
			                                       translateFileMode(mode),
			                                       0, IntPtr.Zero,
			                                       _TransactionHandle,
			                                       IntPtr.Zero, IntPtr.Zero);

			if (fileHandle.IsInvalid)
			{
				var error = Marshal.GetLastWin32Error();
				var baseStr = string.Format("Transaction \"{1}\": Unable to open a file descriptor to \"{0}\".", path, Name ?? "[no name]");

				if (error == ERROR_TRANSACTIONAL_CONFLICT)
					throw new TransactionalConflictException(baseStr 
						+ " You will get this error if you are accessing the transacted file from a non-transacted API before the transaction has " 
						+ "committed. See http://msdn.microsoft.com/en-us/library/aa365536%28VS.85%29.aspx for details.");

				throw new TransactionException(baseStr 
					+ "Please see the inner exceptions for details.", new Win32Exception(Marshal.GetLastWin32Error()));
			}

			return new FileStream(fileHandle, access);
		}

		/// <summary>
		/// Managed -> Native mapping
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		private static NativeFileMode translateFileMode(FileMode mode)
		{
			if (mode != FileMode.Append)
				return (NativeFileMode)(uint)mode;
			return (NativeFileMode)(uint)FileMode.OpenOrCreate;
		}

		/// <summary>
		/// Managed -> Native mapping
		/// </summary>
		/// <param name="access"></param>
		/// <returns></returns>
		private static NativeFileAccess translateFileAccess(FileAccess access)
		{
			switch (access)
			{
				case FileAccess.Read:
					return NativeFileAccess.GenericRead;
				case FileAccess.Write:
					return NativeFileAccess.GenericWrite;
				case FileAccess.ReadWrite:
					return NativeFileAccess.GenericRead | NativeFileAccess.GenericWrite;
				default:
					throw new ArgumentOutOfRangeException("access");
			}
		}

		/// <summary>
		/// Direct Managed -> Native mapping
		/// </summary>
		/// <param name="share"></param>
		/// <returns></returns>
		private static NativeFileShare translateFileShare(FileShare share)
		{
			return (NativeFileShare)(uint)share;
		}

		private bool createDirectoryTransacted(string templatePath,
		                                       string dirPath)
		{
			return CreateDirectoryTransactedW(templatePath,
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
			if (path == null) throw new ArgumentNullException("path");
			if (path == string.Empty) throw new ArgumentException("You can't pass an empty string.");

			return recurseFiles(path,
			                    file => DeleteFileTransactedW(file, _TransactionHandle),
			                    dir => RemoveDirectoryTransactedW(dir, _TransactionHandle));
		}

		private bool recurseFiles(string path,
		                          Func<string, bool> operationOnFiles,
		                          Func<string, bool> operationOnDirectories)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path == string.Empty) throw new ArgumentException("You can't pass an empty string.");

			WIN32_FIND_DATA findData;
			bool addPrefix = !path.StartsWith(@"\\?\");
			bool ok = true;

			string pathWithoutSufflix = addPrefix ? @"\\?\" + Path.GetFullPath(path) : Path.GetFullPath(path);
			path = pathWithoutSufflix + "\\*";

			using (var findHandle = FindFirstFileTransactedW(path, out findData))
			{
				if (findHandle.IsInvalid) return false;

				do
				{
					var subPath = pathWithoutSufflix.Combine(findData.cFileName);

					if ((findData.dwFileAttributes & (uint)FileAttributes.Directory) != 0)
					{
						if (findData.cFileName != "." && findData.cFileName != "..")
							ok &= deleteRecursive(subPath);
					}
					else
						ok = ok && operationOnFiles(subPath);
				}
				while (FindNextFile(findHandle, out findData));
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
			var p = IntPtr.Zero;
			int res = GetFullPathNameTransactedW(dirOrFilePath,
			                                     sb.Capacity,
			                                     sb,
			                                     ref p, // here we can check if it's a file or not.
			                                     _TransactionHandle);

			if (res == 0) // failure
			{
				throw new TransactionException(string.Format("Could not get full path for \"{0}\", see inner exception for details.", 
				                                             dirOrFilePath), 
				                               Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
			}
			if (res > sb.Capacity)
			{
				sb.Capacity = res;	// update capacity
				goto retry;			// handle edge case if the path.Length > 512.
			}

			return sb.ToString();
		}

		// more examples in C++:  
		// http://msdn.microsoft.com/en-us/library/aa364963(VS.85).aspx
		// http://msdn.microsoft.com/en-us/library/x3txb6xc.aspx

		#endregion

		#region Native structures, callbacks and enums

		[Serializable]
		private enum NativeFileMode : uint
		{
			CREATE_NEW = 1,
			CREATE_ALWAYS = 2,
			OPEN_EXISTING = 3,
			OPEN_ALWAYS = 4,
			TRUNCATE_EXISTING = 5
		}

		[Flags, Serializable]
		private enum NativeFileAccess : uint
		{
			GenericRead = 0x80000000,
			GenericWrite = 0x40000000
		}

		/// <summary>
		/// The sharing mode of an object, which can be read, write, both, delete, all of these, or none (refer to the following table).
		/// If this parameter is zero and CreateFileTransacted succeeds, the object cannot be shared and cannot be opened again until the handle is closed. For more information, see the Remarks section of this topic.
		/// You cannot request a sharing mode that conflicts with the access mode that is specified in an open request that has an open handle, because that would result in the following sharing violation: ERROR_SHARING_VIOLATION. For more information, see Creating and Opening Files.
		/// </summary>
		[Flags, Serializable]
		private enum NativeFileShare : uint
		{
			/// <summary>
			/// Disables subsequent open operations on an object to request any type of access to that object.
			/// </summary>
			None = 0x00,

			/// <summary>
			/// Enables subsequent open operations on an object to request read access.
			/// Otherwise, other processes cannot open the object if they request read access.
			/// If this flag is not specified, but the object has been opened for read access, the function fails.
			/// </summary>
			Read = 0x01,

			/// <summary>
			/// Enables subsequent open operations on an object to request write access.
			/// Otherwise, other processes cannot open the object if they request write access.
			/// If this flag is not specified, but the object has been opened for write access or has a file mapping with write access, the function fails.
			/// </summary>
			Write = 0x02,

			/// <summary>
			/// Enables subsequent open operations on an object to request delete access.
			/// Otherwise, other processes cannot open the object if they request delete access.
			/// If this flag is not specified, but the object has been opened for delete access, the function fails.
			/// </summary>
			Delete = 0x04
		}

		enum CopyProgressResult : uint
		{
			PROGRESS_CONTINUE = 0,
			PROGRESS_CANCEL = 1,
			PROGRESS_STOP = 2,
			PROGRESS_QUIET = 3
		}

		enum CopyProgressCallbackReason : uint
		{
			CALLBACK_CHUNK_FINISHED = 0x00000000,
			CALLBACK_STREAM_SWITCH = 0x00000001
		}

		delegate CopyProgressResult CopyProgressRoutine(
			long TotalFileSize,
			long TotalBytesTransferred,
			long StreamSize,
			long StreamBytesTransferred,
			uint dwStreamNumber,
			CopyProgressCallbackReason dwCallbackReason,
			SafeFileHandle hSourceFile,
			SafeFileHandle hDestinationFile,
			IntPtr lpData);

		/// <summary>
		/// This enumeration states options for moving a file.
		/// http://msdn.microsoft.com/en-us/library/aa365241%28VS.85%29.aspx
		/// </summary>
		[Flags, Serializable]
		private enum MoveFileFlags : uint
		{
			/// <summary>
			/// If the file is to be moved to a different volume, the function simulates the move by using the CopyFile  and DeleteFile  functions.
			/// This value cannot be used with MOVEFILE_DELAY_UNTIL_REBOOT.
			/// </summary>
			CopyAllowed = 0x2,

			/// <summary>
			/// Reserved for future use.
			/// </summary>
			CreateHardlink = 0x10,

			/// <summary>
			/// The system does not move the file until the operating system is restarted. The system moves the file immediately after AUTOCHK is executed, but before creating any paging files. Consequently, this parameter enables the function to delete paging files from previous startups.
			/// This value can only be used if the process is in the context of a user who belongs to the administrators group or the LocalSystem account.
			/// This value cannot be used with MOVEFILE_COPY_ALLOWED.
			/// The write operation to the registry value as detailed in the Remarks section is what is transacted. The file move is finished when the computer restarts, after the transaction is complete.
			/// </summary>
			DelayUntilReboot = 0x4,

			/// <summary>
			/// If a file named lpNewFileName exists, the function replaces its contents with the contents of the lpExistingFileName file.
			/// This value cannot be used if lpNewFileName or lpExistingFileName names a directory.
			/// </summary>
			ReplaceExisting = 0x1,

			/// <summary>
			/// A call to MoveFileTransacted means that the move file operation is complete when the commit operation is completed. This flag is unnecessary; there are no negative affects if this flag is specified, other than an operation slowdown. The function does not return until the file has actually been moved on the disk.
			/// Setting this value guarantees that a move performed as a copy and delete operation is flushed to disk before the function returns. The flush occurs at the end of the copy operation.
			/// This value has no effect if MOVEFILE_DELAY_UNTIL_REBOOT is set.
			/// </summary>
			WriteThrough = 0x8
		}
		
#pragma warning disable 1591

		///<summary>
		/// Attributes for security interop.
		///</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
		}

		// The CharSet must match the CharSet of the corresponding PInvoke signature
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct WIN32_FIND_DATA
		{
			public uint dwFileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}

		[Serializable]
		private enum FINDEX_INFO_LEVELS
		{
			FindExInfoStandard = 0,
			FindExInfoMaxInfoLevel = 1
		}

		[Serializable]
		private enum FINDEX_SEARCH_OPS
		{
			FindExSearchNameMatch = 0,
			FindExSearchLimitToDirectories = 1,
			FindExSearchLimitToDevices = 2,
			FindExSearchMaxSearchOp = 3
		}

		#endregion

		#region *FileTransacted[W]

		/*BOOL WINAPI CreateHardLinkTransacted(
		  __in        LPCTSTR lpFileName,
		  __in        LPCTSTR lpExistingFileName,
		  __reserved  LPSECURITY_ATTRIBUTES lpSecurityAttributes,
		  __in        HANDLE hTransaction
		);
		*/
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool CreateHardLinkTransacted([In] string lpFileName,
		                                                    [In] string lpExistingFileName,
		                                                    [In] IntPtr lpSecurityAttributes,
		                                                    [In] SafeTxHandle hTransaction);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool MoveFileTransacted([In] string lpExistingFileName,
		                                              [In] string lpNewFileName, [In] IntPtr lpProgressRoutine,
		                                              [In] IntPtr lpData,
		                                              [In] MoveFileFlags dwFlags,
		                                              [In] SafeTxHandle hTransaction);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern SafeFileHandle CreateFileTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] NativeFileAccess dwDesiredAccess,
			[In] NativeFileShare dwShareMode,
			[In] IntPtr lpSecurityAttributes,
			[In] NativeFileMode dwCreationDisposition,
			[In] uint dwFlagsAndAttributes,
			[In] IntPtr hTemplateFile,
			[In] SafeTxHandle hTransaction,
			[In] IntPtr pusMiniVersion,
			[In] IntPtr pExtendedParameter);

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa363916(VS.85).aspx
		/// </summary>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool DeleteFileTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)] string file,
			SafeTxHandle transaction);

		#endregion

		#region *DirectoryTransacted[W]

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa363857(VS.85).aspx
		/// Creates a new directory as a transacted operation, with the attributes of a specified 
		/// template directory. If the underlying file system supports security on files and 
		/// directories, the function applies a specified security descriptor to the new directory. 
		/// The new directory retains the other attributes of the specified template directory.
		/// </summary>
		/// <param name="lpTemplateDirectory">
		/// The path of the directory to use as a template 
		/// when creating the new directory. This parameter can be NULL.
		/// </param>
		/// <param name="lpNewDirectory">The path of the directory to be created. </param>
		/// <param name="lpSecurityAttributes">A pointer to a SECURITY_ATTRIBUTES structure. The lpSecurityDescriptor member of the structure specifies a security descriptor for the new directory.</param>
		/// <param name="hTransaction">A handle to the transaction. This handle is returned by the CreateTransaction function.</param>
		/// <returns>True if the call succeeds, otherwise do a GetLastError.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CreateDirectoryTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)] string lpTemplateDirectory,
			[MarshalAs(UnmanagedType.LPWStr)] string lpNewDirectory,
			IntPtr lpSecurityAttributes,
			SafeTxHandle hTransaction);

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa365490(VS.85).aspx
		/// Deletes an existing empty directory as a transacted operation.
		/// </summary>
		/// <param name="lpPathName">
		/// The path of the directory to be removed. 
		/// The path must specify an empty directory, 
		/// and the calling process must have delete access to the directory.
		/// </param>
		/// <param name="hTransaction">A handle to the transaction. This handle is returned by the CreateTransaction function.</param>
		/// <returns>True if the call succeeds, otherwise do a GetLastError.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool RemoveDirectoryTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)] string lpPathName,
			SafeTxHandle hTransaction);

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa364966(VS.85).aspx
		/// Retrieves the full path and file name of the specified file as a transacted operation.
		/// </summary>
		/// <remarks>
		/// GetFullPathNameTransacted merges the name of the current drive and directory 
		/// with a specified file name to determine the full path and file name of a 
		/// specified file. It also calculates the address of the file name portion of
		/// the full path and file name. This function does not verify that the 
		/// resulting path and file name are valid, or that they see an existing file 
		/// on the associated volume.
		/// </remarks>
		/// <param name="lpFileName">The name of the file. The file must reside on the local computer; 
		/// otherwise, the function fails and the last error code is set to 
		/// ERROR_TRANSACTIONS_UNSUPPORTED_REMOTE.</param>
		/// <param name="nBufferLength">The size of the buffer to receive the null-terminated string for the drive and path, in TCHARs. </param>
		/// <param name="lpBuffer">A pointer to a buffer that receives the null-terminated string for the drive and path.</param>
		/// <param name="lpFilePart">A pointer to a buffer that receives the address (in lpBuffer) of the final file name component in the path. 
		/// Specify NULL if you do not need to receive this information.
		/// If lpBuffer points to a directory and not a file, lpFilePart receives 0 (zero).</param>
		/// <param name="hTransaction"></param>
		/// <returns>If the function succeeds, the return value is the length, in TCHARs, of the string copied to lpBuffer, not including the terminating null character.</returns>
		[DllImport( "kernel32.dll", CharSet=CharSet.Auto, SetLastError = true)]
		private static extern int GetFullPathNameTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] int nBufferLength,
			[Out] StringBuilder lpBuffer,
			[In, Out] ref IntPtr lpFilePart,
			[In] SafeTxHandle hTransaction);

/*
 * HANDLE WINAPI FindFirstFileTransacted(
  __in        LPCTSTR lpFileName,
  __in        FINDEX_INFO_LEVELS fInfoLevelId,
  __out       LPVOID lpFindFileData,
  __in        FINDEX_SEARCH_OPS fSearchOp,
  __reserved  LPVOID lpSearchFilter,
  __in        DWORD dwAdditionalFlags,
  __in        HANDLE hTransaction
);
*/

		/// <param name="lpFileName"></param>
		/// <param name="fInfoLevelId"></param>
		/// <param name="lpFindFileData"></param>
		/// <param name="fSearchOp">The type of filtering to perform that is different from wildcard matching.</param>
		/// <param name="lpSearchFilter">
		/// A pointer to the search criteria if the specified fSearchOp needs structured search information.
		/// At this time, none of the supported fSearchOp values require extended search information. Therefore, this pointer must be NULL.
		/// </param>
		/// <param name="dwAdditionalFlags">
		/// Specifies additional flags that control the search.
		/// FIND_FIRST_EX_CASE_SENSITIVE = 0x1
		/// Means: Searches are case-sensitive.
		/// </param>
		/// <param name="hTransaction"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern SafeFindHandle FindFirstFileTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] FINDEX_INFO_LEVELS fInfoLevelId, // TODO: Won't work.
			[Out] out WIN32_FIND_DATA lpFindFileData,
			[In] FINDEX_SEARCH_OPS fSearchOp,
			IntPtr lpSearchFilter,
			[In] uint dwAdditionalFlags,
			[In] SafeTxHandle hTransaction);

		private SafeFindHandle findFirstFileTransacted(string filePath, bool directory)
		{
			WIN32_FIND_DATA data;

#if MONO
			uint caseSensitive = 0x1;
#else 
			uint caseSensitive = 0;
#endif

			return FindFirstFileTransactedW(filePath,
			                                FINDEX_INFO_LEVELS.FindExInfoStandard, out data,
			                                directory
			                                	? FINDEX_SEARCH_OPS.FindExSearchLimitToDirectories
			                                	: FINDEX_SEARCH_OPS.FindExSearchNameMatch,
			                                IntPtr.Zero, caseSensitive, _TransactionHandle);
		}

		private SafeFindHandle FindFirstFileTransactedW(string lpFileName,
		                                                out WIN32_FIND_DATA lpFindFileData)
		{
			return FindFirstFileTransactedW(lpFileName, FINDEX_INFO_LEVELS.FindExInfoStandard,
			                                out lpFindFileData, 
			                                FINDEX_SEARCH_OPS.FindExSearchNameMatch, 
			                                IntPtr.Zero, 0,
			                                _TransactionHandle);
		}

		// not transacted
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern bool FindNextFile(SafeFindHandle hFindFile, 
		                                        out WIN32_FIND_DATA lpFindFileData);

		#endregion

		#region Kernel transaction manager

		/// <summary>
		/// Creates a new transaction object. Passing too long a description will cause problems. This behaviour is indeterminate right now.
		/// </summary>
		/// <remarks>
		/// Don't pass unicode to the description (there's no Wide-version of this function
		/// in the kernel).
		/// http://msdn.microsoft.com/en-us/library/aa366011%28VS.85%29.aspx
		/// </remarks>
		/// <param name="lpTransactionAttributes">    
		/// A pointer to a SECURITY_ATTRIBUTES structure that determines whether the returned handle 
		/// can be inherited by child processes. If this parameter is NULL, the handle cannot be inherited.
		/// The lpSecurityDescriptor member of the structure specifies a security descriptor for 
		/// the new event. If lpTransactionAttributes is NULL, the object gets a default 
		/// security descriptor. The access control lists (ACL) in the default security 
		/// descriptor for a transaction come from the primary or impersonation token of the creator.
		/// </param>
		/// <param name="uow">Reserved. Must be zero (0).</param>
		/// <param name="createOptions">
		/// Any optional transaction instructions. 
		/// Value:		TRANSACTION_DO_NOT_PROMOTE
		/// Meaning:	The transaction cannot be distributed.
		/// </param>
		/// <param name="isolationLevel">Reserved; specify zero (0).</param>
		/// <param name="isolationFlags">Reserved; specify zero (0).</param>
		/// <param name="timeout">    
		/// The time, in milliseconds, when the transaction will be aborted if it has not already 
		/// reached the prepared state.
		/// Specify NULL to provide an infinite timeout.
		/// </param>
		/// <param name="description">A user-readable description of the transaction.</param>
		/// <returns>
		/// If the function succeeds, the return value is a handle to the transaction.
		/// If the function fails, the return value is INVALID_HANDLE_VALUE.
		/// </returns>
		[DllImport("ktmw32.dll", SetLastError = true)]
		private static extern IntPtr CreateTransaction(
			IntPtr lpTransactionAttributes,
			IntPtr uow,
			uint createOptions,
			uint isolationLevel,
			uint isolationFlags,
			uint timeout,
			string description);

		private static SafeTxHandle createTransaction(string description)
		{
			return new SafeTxHandle(CreateTransaction(IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, description));
		}

		/// <summary>
		/// Requests that the specified transaction be committed.
		/// </summary>
		/// <remarks>You can commit any transaction handle that has been opened 
		/// or created using the TRANSACTION_COMMIT permission; any application can 
		/// commit a transaction, not just the creator.
		/// This function can only be called if the transaction is still active, 
		/// not prepared, pre-prepared, or rolled back.</remarks>
		/// <param name="transaction">
		/// This handle must have been opened with the TRANSACTION_COMMIT access right. 
		/// For more information, see KTM Security and Access Rights.</param>
		/// <returns></returns>
		[DllImport("ktmw32.dll", SetLastError = true)]
		private static extern bool CommitTransaction(SafeTxHandle transaction);

		/// <summary>
		/// Requests that the specified transaction be rolled back. This function is synchronous.
		/// </summary>
		/// <param name="transaction">A handle to the transaction.</param>
		/// <returns>If the function succeeds, the return value is nonzero.</returns>
		[DllImport("ktmw32.dll", SetLastError = true)]
		private static extern bool RollbackTransaction(SafeTxHandle transaction);

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