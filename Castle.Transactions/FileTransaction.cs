using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Transactions;
using Castle.IO;
using Castle.IO.FileSystems.Local.Win32.Interop;
using Castle.Transactions.Internal;
using Castle.Transactions.IO;
using NativeMethods = Castle.Transactions.Internal.NativeMethods;
using Path = Castle.IO.Path;

namespace Castle.Transactions
{
	///<summary>
	///	Represents a transaction on transactional kernels
	///	like the Vista kernel or Server 2008 kernel and newer.
	///</summary>
	///<remarks>
	///	Good information for dealing with the peculiarities of the runtime:
	///	http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.aspx
	///</remarks>
	internal sealed class FileTransaction : IFileAdapter, IDirectoryAdapter, ITransaction
	{
		private readonly ITransaction _Inner;
		private readonly string _Name;
		private SafeKernelTransactionHandle _TransactionHandle;
		private TransactionState _State;

		#region Constructors

		public FileTransaction()
			: this(null)
		{
		}

		public FileTransaction(string name)
		{
			_Name = name;
		
			InnerBegin();

			_State = TransactionState.Active;
		}

		public FileTransaction(string name, CommittableTransaction inner, uint stackDepth, ITransactionOptions creationOptions,
		                       Action onDispose)
		{
			Contract.Requires(inner != null);
			Contract.Requires(creationOptions != null);

			_Inner = new Transaction(inner, stackDepth, creationOptions, onDispose);

			_Name = name;
			InnerBegin();
		}

		public FileTransaction(string name, DependentTransaction inner, uint stackDepth, ITransactionOptions creationOptions,
		                       Action onDispose)
		{
			Contract.Requires(inner != null);
			Contract.Requires(creationOptions != null);

			_Inner = new Transaction(inner, stackDepth, creationOptions, onDispose);

			_Name = name;
			InnerBegin();
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Inner == null || _State == _Inner.State);
		}

		#endregion

		///<summary>
		///	Gets the name of the transaction.
		///</summary>
		public string Name
		{
			get { return _Name ?? string.Format("FileTransaction#{0}", GetHashCode()); }
		}

		private void InnerBegin()
		{
			Contract.Ensures(_State == TransactionState.Active);
			// we have a ongoing current transaction, join it!
			if (System.Transactions.Transaction.Current != null)
			{
				var ktx = (IKernelTransaction) TransactionInterop.GetDtcTransaction(System.Transactions.Transaction.Current);
				
				SafeKernelTransactionHandle handle;
				ktx.GetHandle(out handle);

				// even though _TransactionHandle can already contain a handle if this thread 
				// had been yielded just before setting this reference, the "safe"-ness of the wrapper should
				// not dispose the other handle which is now removed
				_TransactionHandle = handle;
				//IsAmbient = true; // TODO: Perhaps we created this item and we need to notify the transaction manager...
			}
			else _TransactionHandle = NativeMethods.CreateTransaction(string.Format("{0} Transaction", _Name));

			if (!_TransactionHandle.IsInvalid)
			{
				_State = TransactionState.Active;
				return;
			}

			throw new TransactionException(
				"Cannot begin file transaction. CreateTransaction failed and there's no ambient transaction.",
				LastEx());
		}

		TransactionState ITransaction.State
		{
			get { return _Inner != null ? _Inner.State : _State; }
		}

		ITransactionOptions ITransaction.CreationOptions
		{
			get { return _Inner != null ? _Inner.CreationOptions : new DefaultTransactionOptions(); }
		}

		System.Transactions.Transaction ITransaction.Inner
		{
			get { return _Inner != null ? _Inner.Inner : null; }
		}

		Maybe<SafeKernelTransactionHandle> ITransaction.KernelTransactionHandle
		{
			get
			{
				return _TransactionHandle != null && !_TransactionHandle.IsInvalid
				       	? Maybe.Some(_TransactionHandle)
				       	: Maybe.None<SafeKernelTransactionHandle>();
			}
		}

		//Maybe<IRetryPolicy> ITransaction.FailedPolicy
		//{
		//    get { return _Inner != null ? _Inner.FailedPolicy : Maybe.None<IRetryPolicy>(); }
		//}

		string ITransaction.LocalIdentifier
		{
			get { return _Inner != null ? _Inner.LocalIdentifier : _Name; }
		}

		void ITransaction.Rollback()
		{
			try
			{
				if (!NativeMethods.RollbackTransaction(_TransactionHandle))
					throw new TransactionException("Rollback failed.", LastEx());

				if (_Inner != null)
					_Inner.Rollback();
			}
			finally
			{
				_State = TransactionState.Aborted;
			}
		}

		void ITransaction.Complete()
		{
			try
			{
				if (_Inner != null && _Inner.State == TransactionState.Active)
					_Inner.Complete();

				if (!NativeMethods.CommitTransaction(_TransactionHandle))
					throw new TransactionException("Commit failed.", LastEx());

				_State = TransactionState.CommittedOrCompleted;
			}
			finally
			{
				_State = _State != TransactionState.CommittedOrCompleted
				         	? TransactionState.Aborted
				         	: TransactionState.CommittedOrCompleted;
			}
		}

		#region State - defensive programming 

		private void AssertState(TransactionState status, string msg = null)
		{
			if (status == _State)
				return;
			
			if (!string.IsNullOrEmpty(msg))
				throw new TransactionException(msg);

			throw new TransactionException(string.Format("State failure; should have been {0} but was {1}",
			                                             status, _State));
		}

		#endregion

		#region IFileAdapter members

		FileStream IFileAdapter.Create(string path)
		{
			AssertState(TransactionState.Active);

			return Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		void IFileAdapter.Delete(string filePath)
		{
			AssertState(TransactionState.Active);

			if (!NativeMethods.DeleteFileTransactedW(filePath, _TransactionHandle))
				throw new TransactionException("Unable to perform transacted file delete.", LastEx());
		}

		FileStream IFileAdapter.Open(string filePath, FileMode mode)
		{
			return Open(filePath, mode, FileAccess.ReadWrite, FileShare.None);
		}

		FileStream IFileAdapter.OpenWrite(string path)
		{
			return Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
		}

		//int IFileAdapter.WriteStream(string targetPath, Stream sourceStream)
		//{
		//    throw new NotSupportedException("Use the file adapter instead!");
		//}

		//string IFileAdapter.ReadAllText(string path, Encoding encoding)
		//{
		//    AssertState(TransactionState.Active);

		//    using (var reader = new StreamReader(Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), encoding))
		//    {
		//        return reader.ReadToEnd();
		//    }
		//}

		void IFileAdapter.Move(string originalFilePath, string newFilePath)
		{
			// case 1, the new file path is a folder
			if (((IDirectoryAdapter) this).Exists(newFilePath))
			{
				var fileName = Path.GetFileName(originalFilePath);
				Contract.Assume(fileName.Length > 0, "by pre-condition of IFileAdapterContract.Move");
				NativeMethods.MoveFileTransacted(originalFilePath, System.IO.Path.Combine(newFilePath, fileName), IntPtr.Zero,
				                                 IntPtr.Zero, MoveFileFlags.CopyAllowed,
				                                 _TransactionHandle);
				return;
			}

			// case 2, its not a folder, so assume it's a file.
			NativeMethods.MoveFileTransacted(originalFilePath, newFilePath, IntPtr.Zero, IntPtr.Zero,
			                                 MoveFileFlags.CopyAllowed,
			                                 _TransactionHandle);
		}

		public void Move(string originalPath, string newPath, bool overwrite)
		{
			var flags = MoveFileFlags.CopyAllowed;
			if (overwrite)
				flags |= MoveFileFlags.ReplaceExisting;
			NativeMethods.MoveFileTransacted(originalPath, newPath, IntPtr.Zero, IntPtr.Zero, flags, _TransactionHandle);
		}

		bool IFileAdapter.Exists(string filePath)
		{
			AssertState(TransactionState.Active);

			using (var handle = NativeMethods.FindFirstFileTransacted(filePath, false, _TransactionHandle))
				return !handle.IsInvalid;
		}

		//string IFileAdapter.ReadAllText(string path)
		//{
		//    AssertState(TransactionState.Active);

		//    using (var reader = new StreamReader(Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
		//        return reader.ReadToEnd();
		//}

		//void IFileAdapter.WriteAllText(string targetPath, string contents)
		//{
		//    AssertState(TransactionState.Active);

		//    var exists = ((IFileAdapter) this).Exists(targetPath);
		//    var fileMode = exists ? FileMode.Truncate : FileMode.OpenOrCreate;
			
		//    using (var writer = new StreamWriter(Open(targetPath, fileMode, FileAccess.Write, FileShare.None)))
		//        writer.Write(contents);
		//}

		//IEnumerable<string> IFileAdapter.ReadAllLines(string filePath)
		//{
		//    throw new NotImplementedException();
		//}

		StreamWriter IFileAdapter.CreateText(string filePath)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDirectoryAdapter members

		///<summary>
		///	Creates a directory at the path given.
		///</summary>
		///<param name = "path">The path to create the directory at.</param>
		// TODO: Move this method to Create() on IDirectory.
		bool IDirectoryAdapter.Create(string path)
		{
			AssertState(TransactionState.Active);

			path = Path.NormDirSepChars(CleanPathEnd(path));

			// we don't need to re-create existing folders.
			if (((IDirectoryAdapter) this).Exists(path))
				return true;

			var nonExistent = new Stack<string>();
			nonExistent.Push(path);

			var curr = path;
			while (!((IDirectoryAdapter) this).Exists(curr)
			       && (curr.Contains(Path.DirectorySeparatorChar)
			           || curr.Contains(Path.AltDirectorySeparatorChar)))
			{
				curr = Path.GetPathWithoutLastBit(curr).ToString();

				if (!((IDirectoryAdapter) this).Exists(curr))
					nonExistent.Push(curr);
			}

			while (nonExistent.Count > 0)
			{
				if (!CreateDirectoryTransacted(nonExistent.Pop()))
				{
					throw new TransactionException(string.Format("Failed to create directory \"{1}\" at path \"{0}\". "
					                                             + "See inner exception for more details.", path, curr),
					                               LastEx());
				}
			}

			return false;
		}

		/// <summary>
		/// 	Deletes a folder recursively.
		/// </summary>
		/// <param name = "path">The directory path to start deleting at!</param>
		void IDirectoryAdapter.Delete(string path)
		{
			AssertState(TransactionState.Active);

			if (!NativeMethods.RemoveDirectoryTransactedW(path, _TransactionHandle))
				throw new TransactionException("Unable to delete folder. See inner exception for details.",
				                               LastEx());
		}

		/// <summary>
		/// 	Checks whether the path exists.
		/// </summary>
		/// <param name = "path">Path to check.</param>
		/// <returns>True if it exists, false otherwise.</returns>
		bool IDirectoryAdapter.Exists(string path)
		{
			AssertState(TransactionState.Active);

			path = CleanPathEnd(path);

			using (var handle = NativeMethods.FindFirstFileTransacted(path, true, _TransactionHandle))
				return !handle.IsInvalid;
		}

		string IDirectoryAdapter.GetFullPath(string relativePath)
		{
			AssertState(TransactionState.Active);
			return GetFullPathNameTransacted(relativePath);
		}

		string IDirectoryAdapter.MapPath(string path)
		{
			throw new NotSupportedException("Implemented on the directory adapter.");
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath)
		{
			var da = ((IDirectoryAdapter) this);

			if (!da.Exists(originalPath))
				throw new DirectoryNotFoundException(
					string.Format("The path \"{0}\" could not be found. The source directory needs to exist.",
					              originalPath));

			if (!da.Exists(newPath))
				da.Create(newPath);

			if (!NativeMethods.MoveFileTransacted(originalPath, newPath, IntPtr.Zero, IntPtr.Zero,
			                                      MoveFileFlags.ReplaceExisting, _TransactionHandle))
				throw new TransactionException("Could not move directory", LastEx());

			//RecurseFiles(originalPath, f => { Console.WriteLine("file: {0}", f); return true; }, 
			//    d => { Console.WriteLine("dir: {0}", d);  return true; });
		}

		/// <summary>
		/// 	Deletes an empty directory
		/// </summary>
		/// <param name = "path">The path to the folder to delete.</param>
		/// <param name = "recursively">
		/// 	Whether to delete recursively or not.
		/// 	When recursive, we delete all subfolders and files in the given
		/// 	directory as well.
		/// </param>
		bool IDirectoryAdapter.Delete(string path, bool recursively)
		{
			AssertState(TransactionState.Active);
			return recursively
			       	? DeleteRecursive(path)
			       	: NativeMethods.RemoveDirectoryTransactedW(path, _TransactionHandle);
		}

		#endregion

		#region Dispose-pattern

		void ITransaction.Dispose()
		{
			Dispose();
		}

		/// <summary>
		/// 	Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);

			// the base transaction dispose all resources active, so we must be careful
			// and call our own resources first, thereby having to call this afterwards.
			//base.Dispose();

			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			// no unmanaged code here, just return.
			if (!disposing) return;

			if (_State == TransactionState.Disposed) return;
			// called via the Dispose() method on IDisposable, 
			// can use private object references.

			try
			{
				if (_State == TransactionState.Active)
					((ITransaction) this).Rollback();

				if (_TransactionHandle != null)
					_TransactionHandle.Dispose();

				if (_Inner != null)
					_Inner.Dispose();
			}
			finally
			{
				_State = TransactionState.Disposed;
			}
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// 	Creates a file handle with the current ongoing transaction.
		/// </summary>
		/// <param name = "path">The path of the file.</param>
		/// <param name = "mode">The file mode, i.e. what is going to be done if it exists etc.</param>
		/// <param name = "access">The access rights this handle has.</param>
		/// <param name = "share">What other handles may be opened; sharing settings.</param>
		/// <returns>A safe file handle. Not null, but may be invalid.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
			Justification = "This method's aim IS to provide a disposable resource.")]
		private FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			// Future: Support System.IO.FileOptions which is the dwFlagsAndAttribute parameter.
			var fileHandle = NativeMethods.CreateFileTransactedW(path,
			                                                     access.ToNative(),
			                                                     share.ToNative(),
			                                                     IntPtr.Zero,
			                                                     mode.ToNative(),
			                                                     0, IntPtr.Zero,
			                                                     _TransactionHandle, 
			                                                     IntPtr.Zero, IntPtr.Zero);
			var error = Marshal.GetLastWin32Error();

			if (fileHandle.IsInvalid)
			{
				var baseStr = string.Format("Transaction \"{1}\": Unable to open a file descriptor to \"{0}\".", path,
				                            Name ?? "[no name]");

				if (error == TxFCodes.ERROR_TRANSACTIONAL_CONFLICT)
					throw new TransactionalConflictException(baseStr
					                                         +
					                                         " You will get this error if you are accessing the transacted file from a non-transacted API before the transaction has "
					                                         + "committed. See HelperLink for details.",
					                                         new Uri("http://msdn.microsoft.com/en-us/library/aa365536%28VS.85%29.aspx"));

				throw new TransactionException(baseStr + "Please see the inner exceptions for details.",
				                               LastEx());
			}

			return new FileStream(fileHandle, access);
		}

		private bool CreateDirectoryTransacted(string templatePath,
		                                       string dirPath)
		{
			return NativeMethods.CreateDirectoryTransactedW(templatePath,
			                                                dirPath,
			                                                IntPtr.Zero,
			                                                _TransactionHandle);
		}

		private bool CreateDirectoryTransacted(string dirPath)
		{
			return CreateDirectoryTransacted(null, dirPath);
		}

		private bool DeleteRecursive(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			return RecurseFiles(path,
			                    file => NativeMethods.DeleteFileTransactedW(file, _TransactionHandle),
			                    dir => NativeMethods.RemoveDirectoryTransactedW(dir, _TransactionHandle));
		}

		private bool RecurseFiles(string path,
		                          Func<string, bool> operationOnFiles,
		                          Func<string, bool> operationOnDirectories)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			WIN32_FIND_DATA findData;
			var addPrefix = !path.StartsWith(@"\\?\");
			var ok = true;

			var pathWithoutSufflix = addPrefix ? @"\\?\" + Path.GetFullPath(path) : Path.GetFullPath(path);
			path = pathWithoutSufflix + "\\*";

			using (var findHandle = NativeMethods.FindFirstFileTransactedW(path, _TransactionHandle, out findData))
			{
				if (findHandle.IsInvalid) return false;

				do
				{
					Contract.Assume(!string.IsNullOrEmpty(findData.cFileName) && findData.cFileName.Length > 0,
					                "or otherwise FindNextFile should have returned false");

					var subPath = System.IO.Path.Combine(pathWithoutSufflix, findData.cFileName);

					if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
					{
						if (findData.cFileName != "." && findData.cFileName != "..")
							ok &= DeleteRecursive(subPath);
					}
					else
						ok = ok && operationOnFiles(subPath);
				} while (Castle.IO.FileSystems.Local.Win32.Interop.NativeMethods.FindNextFile(findHandle, out findData));
			}

			return ok && operationOnDirectories(pathWithoutSufflix);
		}

		private string GetFullPathNameTransacted(string dirOrFilePath)
		{
			var sb = new StringBuilder(512);

			retry:
			var p = IntPtr.Zero;
			var res = NativeMethods.GetFullPathNameTransactedW(dirOrFilePath,
			                                                   sb.Capacity,
			                                                   sb,
			                                                   ref p, // here we can check if it's a file or not.
			                                                   _TransactionHandle);

			if (res == 0) // failure
			{
				throw new TransactionException(
					string.Format("Could not get full path for \"{0}\", see inner exception for details.",
					              dirOrFilePath), LastEx());
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

		private static string CleanPathEnd(string path)
		{
			return path.TrimEnd('/', '\\');
		}

		private static Exception LastEx()
		{
			return Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
		}
	}
}