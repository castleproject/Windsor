using System;
using System.Runtime.InteropServices;
using System.Text;
using Castle.Services.Transaction.IO;
using Microsoft.Win32.SafeHandles;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Castle.Services.Transaction.Internal
{
	internal static class NativeMethods
	{
		#region Kernel transaction manager

		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedMember.Local

		// overview here: http://msdn.microsoft.com/en-us/library/aa964885(VS.85).aspx
		// helper: http://www.improve.dk/blog/2009/02/14/utilizing-transactional-ntfs-through-dotnet


		/* BOOL WINAPI CloseHandle(
		 *		__in  HANDLE hObject
		 * );
		 */
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FindClose(SafeHandle hFindFile);

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
		internal static extern IntPtr CreateTransaction(
			IntPtr lpTransactionAttributes,
			IntPtr uow,
			uint createOptions,
			uint isolationLevel,
			uint isolationFlags,
			uint timeout,
			string description);

		internal static SafeKernelTransactionHandle createTransaction(string description)
		{
			return new SafeKernelTransactionHandle(CreateTransaction(IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, description));
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
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("ktmw32.dll", SetLastError = true)]
		internal static extern bool CommitTransaction(SafeKernelTransactionHandle transaction);

		/// <summary>
		/// Requests that the specified transaction be rolled back. This function is synchronous.
		/// </summary>
		/// <param name="transaction">A handle to the transaction.</param>
		/// <returns>If the function succeeds, the return value is nonzero.</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("ktmw32.dll", SetLastError = true)]
		internal static extern bool RollbackTransaction(SafeKernelTransactionHandle transaction);

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
		internal static extern bool CreateHardLinkTransacted([In] string lpFileName,
															[In] string lpExistingFileName,
															[In] IntPtr lpSecurityAttributes,
															[In] SafeKernelTransactionHandle hTransaction);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool MoveFileTransacted([In] string lpExistingFileName,
													  [In] string lpNewFileName, [In] IntPtr lpProgressRoutine,
													  [In] IntPtr lpData,
													  [In] MoveFileFlags dwFlags,
													  [In] SafeKernelTransactionHandle hTransaction);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern SafeFileHandle CreateFileTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] NativeFileAccess dwDesiredAccess,
			[In] NativeFileShare dwShareMode,
			[In] IntPtr lpSecurityAttributes,
			[In] NativeFileMode dwCreationDisposition,
			[In] uint dwFlagsAndAttributes,
			[In] IntPtr hTemplateFile,
			[In] SafeKernelTransactionHandle hTransaction,
			[In] IntPtr pusMiniVersion,
			[In] IntPtr pExtendedParameter);

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa363916(VS.85).aspx
		/// </summary>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool DeleteFileTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)] string file,
			SafeKernelTransactionHandle transaction);

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

		/// <summary>
		/// Continues a file search from a previous call to the FindFirstFile or FindFirstFileEx function.
		/// If there is a transaction bound to the file enumeration handle, then the files that are returned are subject to transaction isolation rules.
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/aa364428%28v=VS.85%29.aspx</remarks>
		/// <param name="hFindFile">The search handle returned by a previous call to the FindFirstFile or FindFirstFileEx function.</param>
		/// <param name="lpFindFileData">    A pointer to the WIN32_FIND_DATA structure that receives information about the found file or subdirectory.
		/// The structure can be used in subsequent calls to FindNextFile to indicate from which file to continue the search.
		/// </param>
		/// <returns>If the function succeeds, the return value is nonzero and the lpFindFileData parameter contains information about the next file or directory found.
		/// If the function fails, the return value is zero and the contents of lpFindFileData are indeterminate. To get extended error information, call the GetLastError function.
		/// If the function fails because no more matching files can be found, the GetLastError function returns ERROR_NO_MORE_FILES.</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern bool FindNextFile([In] SafeFindHandle hFindFile,
		                                         [Out] out WIN32_FIND_DATA lpFindFileData);

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
			[In] SafeKernelTransactionHandle hTransaction);


		/// <summary>
		/// Not extern
		/// </summary>
		internal static SafeFindHandle FindFirstFileTransacted(string filePath, bool directory, SafeKernelTransactionHandle kernelTxHandle)
		{
			WIN32_FIND_DATA data;

#if MONO
			const uint caseSensitive = 0x1;
#else
			const uint caseSensitive = 0;
#endif

			return FindFirstFileTransactedW(filePath,
											FINDEX_INFO_LEVELS.FindExInfoStandard, out data,
											directory
												? FINDEX_SEARCH_OPS.FindExSearchLimitToDirectories
												: FINDEX_SEARCH_OPS.FindExSearchNameMatch,
											IntPtr.Zero, caseSensitive, kernelTxHandle);
		}

		/// <summary>
		/// Not extern
		/// </summary>
		internal static SafeFindHandle FindFirstFileTransactedW(string lpFileName,
			SafeKernelTransactionHandle kernelTxHandle, out WIN32_FIND_DATA lpFindFileData)
		{
			return FindFirstFileTransactedW(lpFileName, FINDEX_INFO_LEVELS.FindExInfoStandard,
											out lpFindFileData,
											FINDEX_SEARCH_OPS.FindExSearchNameMatch,
											IntPtr.Zero, 0,
											kernelTxHandle);
		}

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
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool CreateDirectoryTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)] string lpTemplateDirectory,
			[MarshalAs(UnmanagedType.LPWStr)] string lpNewDirectory,
			IntPtr lpSecurityAttributes,
			SafeKernelTransactionHandle hTransaction);

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
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool RemoveDirectoryTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)] string lpPathName,
			SafeKernelTransactionHandle hTransaction);

		/*
		 * Might need to use:
		 * DWORD WINAPI GetLongPathNameTransacted(
		 *	  __in   LPCTSTR lpszShortPath,
		 *	  __out  LPTSTR lpszLongPath,
		 *	  __in   DWORD cchBuffer,
		 *	  __in   HANDLE hTransaction
		 *	);
		 */

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
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetFullPathNameTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] int nBufferLength,
			[Out] StringBuilder lpBuffer,
			[In, Out] ref IntPtr lpFilePart,
			[In] SafeKernelTransactionHandle hTransaction);

		#endregion
		#region Native structures, callbacks and enums

		[Serializable]
		internal enum NativeFileMode : uint
		{
			CREATE_NEW = 1,
			CREATE_ALWAYS = 2,
			OPEN_EXISTING = 3,
			OPEN_ALWAYS = 4,
			TRUNCATE_EXISTING = 5
		}

		[Flags, Serializable]
		internal enum NativeFileAccess : uint
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
		internal enum NativeFileShare : uint
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

		internal enum CopyProgressResult : uint
		{
			PROGRESS_CONTINUE = 0,
			PROGRESS_CANCEL = 1,
			PROGRESS_STOP = 2,
			PROGRESS_QUIET = 3
		}

		internal enum CopyProgressCallbackReason : uint
		{
			CALLBACK_CHUNK_FINISHED = 0x00000000,
			CALLBACK_STREAM_SWITCH = 0x00000001
		}

		internal delegate CopyProgressResult CopyProgressRoutine(
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
		internal enum MoveFileFlags : uint
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
		internal struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
		}

		// The CharSet must match the CharSet of the corresponding PInvoke signature
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct WIN32_FIND_DATA
		{
			public readonly uint dwFileAttributes;
			public readonly FILETIME ftCreationTime;
			public readonly FILETIME ftLastAccessTime;
			public readonly FILETIME ftLastWriteTime;
			public readonly uint nFileSizeHigh;
			public readonly uint nFileSizeLow;
			public readonly uint dwReserved0;
			public readonly uint dwReserved1;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public readonly string cFileName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public readonly string cAlternateFileName;
		}

		[Serializable]
		internal enum FINDEX_INFO_LEVELS
		{
			FindExInfoStandard = 0,
			FindExInfoMaxInfoLevel = 1
		}

		[Serializable]
		internal enum FINDEX_SEARCH_OPS
		{
			FindExSearchNameMatch = 0,
			FindExSearchLimitToDirectories = 1,
			FindExSearchLimitToDevices = 2,
			FindExSearchMaxSearchOp = 3
		}

		#endregion
	}
}