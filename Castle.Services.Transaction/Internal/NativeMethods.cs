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
using System.Runtime.InteropServices;
using System.Text;
using Castle.IO;
using Castle.IO.FileSystems.Local.Win32.Interop;
using Castle.Transactions.IO;
using Microsoft.Win32.SafeHandles;

namespace Castle.Transactions.Internal
{
	internal static class NativeMethods
	{
		/// <summary>
		/// 	http://msdn.microsoft.com/en-us/library/aa364966(VS.85).aspx
		/// 	Retrieves the full path and file name of the specified file as a transacted operation.
		/// </summary>
		/// <remarks>
		/// 	GetFullPathNameTransacted merges the name of the current drive and directory 
		/// 	with a specified file name to determine the full path and file name of a 
		/// 	specified file. It also calculates the address of the file name portion of
		/// 	the full path and file name. This function does not verify that the 
		/// 	resulting path and file name are valid, or that they see an existing file 
		/// 	on the associated volume.
		/// </remarks>
		/// <param name = "lpFileName">The name of the file. The file must reside on the local computer; 
		/// 	otherwise, the function fails and the last error code is set to 
		/// 	ERROR_TRANSACTIONS_UNSUPPORTED_REMOTE.</param>
		/// <param name = "nBufferLength">The size of the buffer to receive the null-terminated string for the drive and path, in TCHARs. </param>
		/// <param name = "lpBuffer">A pointer to a buffer that receives the null-terminated string for the drive and path.</param>
		/// <param name = "lpFilePart">A pointer to a buffer that receives the address (in lpBuffer) of the final file name component in the path. 
		/// 	Specify NULL if you do not need to receive this information.
		/// 	If lpBuffer points to a directory and not a file, lpFilePart receives 0 (zero).</param>
		/// <param name = "hTransaction"></param>
		/// <returns>If the function succeeds, the return value is the length, in TCHARs, of the string copied to lpBuffer, not including the terminating null character.</returns>
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int GetFullPathNameTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] int nBufferLength,
			[Out] StringBuilder lpBuffer,
			[In, Out] ref IntPtr lpFilePart,
			[In] SafeKernelTransactionHandle hTransaction);


		// http://msdn.microsoft.com/en-us/library/aa363916(VS.85).aspx
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteFileTransactedW([MarshalAs(UnmanagedType.LPWStr)] string file, SafeKernelTransactionHandle transaction);

		
		// http://msdn.microsoft.com/en-us/library/aa363859%28v=vs.85%29.aspx
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern SafeFileHandle CreateFileTransactedW(
			[In] string lpFileName,
			[In] NativeFileAccess dwDesiredAccess,
			[In] NativeFileShare dwShareMode,
			[In, Optional] IntPtr lpSecurityAttributes,
			[In] NativeFileMode dwCreationDisposition,
			[In] NativeFileOptions dwFlagsAndAttributes,
			[In, Optional] IntPtr hTemplateFile,
			[In] SafeKernelTransactionHandle hTransaction,
			[In, Optional] IntPtr pusMiniVersion,
			IntPtr pExtendedParameter);



		/// <summary>
		/// 	Creates a new transaction object. Passing too long a description will cause problems. This behaviour is indeterminate right now.
		/// </summary>
		/// <remarks>
		/// 	Don't pass unicode to the description (there's no Wide-version of this function
		/// 	in the kernel).
		/// 	http://msdn.microsoft.com/en-us/library/aa366011%28VS.85%29.aspx
		/// </remarks>
		/// <param name = "lpTransactionAttributes">    
		/// 	A pointer to a SECURITY_ATTRIBUTES structure that determines whether the returned handle 
		/// 	can be inherited by child processes. If this parameter is NULL, the handle cannot be inherited.
		/// 	The lpSecurityDescriptor member of the structure specifies a security descriptor for 
		/// 	the new event. If lpTransactionAttributes is NULL, the object gets a default 
		/// 	security descriptor. The access control lists (ACL) in the default security 
		/// 	descriptor for a transaction come from the primary or impersonation token of the creator.
		/// </param>
		/// <param name = "uow">Reserved. Must be zero (0).</param>
		/// <param name = "createOptions">
		/// 	Any optional transaction instructions. 
		/// 	Value:		TRANSACTION_DO_NOT_PROMOTE
		/// 	Meaning:	The transaction cannot be distributed.
		/// </param>
		/// <param name = "isolationLevel">Reserved; specify zero (0).</param>
		/// <param name = "isolationFlags">Reserved; specify zero (0).</param>
		/// <param name = "timeout">    
		/// 	The time, in milliseconds, when the transaction will be aborted if it has not already 
		/// 	reached the prepared state.
		/// 	Specify NULL to provide an infinite timeout.
		/// </param>
		/// <param name = "description">A user-readable description of the transaction.</param>
		/// <returns>
		/// 	If the function succeeds, the return value is a handle to the transaction.
		/// 	If the function fails, the return value is INVALID_HANDLE_VALUE.
		/// </returns>
		[DllImport("ktmw32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr CreateTransaction(IntPtr lpTransactionAttributes, IntPtr uow, uint createOptions,
			uint isolationLevel, uint isolationFlags, uint timeout, string description);

		internal static SafeKernelTransactionHandle CreateTransaction(string description)
		{
			return new SafeKernelTransactionHandle(CreateTransaction(IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, description));
		}

		/// <summary>
		/// 	Requests that the specified transaction be committed.
		/// </summary>
		/// <remarks>
		/// 	You can commit any transaction handle that has been opened 
		/// 	or created using the TRANSACTION_COMMIT permission; any application can 
		/// 	commit a transaction, not just the creator.
		/// 	This function can only be called if the transaction is still active, 
		/// 	not prepared, pre-prepared, or rolled back.
		/// </remarks>
		/// <param name = "transaction">
		/// 	This handle must have been opened with the TRANSACTION_COMMIT access right. 
		/// 	For more information, see KTM Security and Access Rights.</param>
		/// <returns></returns>
		[DllImport("ktmw32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CommitTransaction(SafeKernelTransactionHandle transaction);

		/// <summary>
		/// 	Requests that the specified transaction be rolled back. This function is synchronous.
		/// </summary>
		/// <param name = "transaction">A handle to the transaction.</param>
		/// <returns>If the function succeeds, the return value is nonzero.</returns>
		[DllImport("ktmw32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool RollbackTransaction(SafeKernelTransactionHandle transaction);

		#region *FileTransacted[W]

		/*BOOL WINAPI CreateHardLinkTransacted(
		  __in        LPCTSTR lpFileName,
		  __in        LPCTSTR lpExistingFileName,
		  __reserved  LPSECURITY_ATTRIBUTES lpSecurityAttributes,
		  __in        HANDLE hTransaction
		);
		*/

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CreateHardLinkTransacted([In] string lpFileName,
															 [In] string lpExistingFileName,
															 [In] IntPtr lpSecurityAttributes,
															 [In] SafeKernelTransactionHandle hTransaction);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool MoveFileTransacted([In] string lpExistingFileName,
													   [In] string lpNewFileName,
													   [In] IntPtr lpProgressRoutine,
													   [In] IntPtr lpData,
													   [In] MoveFileFlags dwFlags,
													   [In] SafeKernelTransactionHandle hTransaction);


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

		/// <param name = "lpFileName"></param>
		/// <param name = "fInfoLevelId"></param>
		/// <param name = "lpFindFileData"></param>
		/// <param name = "fSearchOp">The type of filtering to perform that is different from wildcard matching.</param>
		/// <param name = "lpSearchFilter">
		/// 	A pointer to the search criteria if the specified fSearchOp needs structured search information.
		/// 	At this time, none of the supported fSearchOp values require extended search information. Therefore, this pointer must be NULL.
		/// </param>
		/// <param name = "dwAdditionalFlags">
		/// 	Specifies additional flags that control the search.
		/// 	FIND_FIRST_EX_CASE_SENSITIVE = 0x1
		/// 	Means: Searches are case-sensitive.
		/// </param>
		/// <param name = "hTransaction"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern SafeFindHandle FindFirstFileTransactedW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] FINDEX_INFO_LEVELS fInfoLevelId, // TODO: Won't work.
			[Out] out WIN32_FIND_DATA lpFindFileData,
			[In] FINDEX_SEARCH_OPS fSearchOp,
			IntPtr lpSearchFilter,
			[In] uint dwAdditionalFlags,
			[In] SafeKernelTransactionHandle hTransaction);


		/// <summary>
		/// 	Not extern
		/// </summary>
		internal static SafeFindHandle FindFirstFileTransacted(string filePath, bool directory,
															   SafeKernelTransactionHandle kernelTxHandle)
		{
			WIN32_FIND_DATA data;
#if MONO
			const uint caseSensitive = 0x1;
#else
			const uint caseSensitive = 0;
#endif

			return FindFirstFileTransactedW(filePath, FINDEX_INFO_LEVELS.FindExInfoStandard, out data,
											directory
												? FINDEX_SEARCH_OPS.FindExSearchLimitToDirectories
												: FINDEX_SEARCH_OPS.FindExSearchNameMatch,
											IntPtr.Zero, caseSensitive, kernelTxHandle);
		}

		/// <summary>
		/// 	Not extern
		/// </summary>
		internal static SafeFindHandle FindFirstFileTransactedW(string lpFileName,
																SafeKernelTransactionHandle kernelTxHandle,
																out WIN32_FIND_DATA lpFindFileData)
		{
			return FindFirstFileTransactedW(lpFileName, FINDEX_INFO_LEVELS.FindExInfoStandard, out lpFindFileData,
											FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, 0, kernelTxHandle);
		}

		#endregion

		#region *DirectoryTransacted[W]

		/// <summary>
		/// 	http://msdn.microsoft.com/en-us/library/aa363857(VS.85).aspx
		/// 	Creates a new directory as a transacted operation, with the attributes of a specified 
		/// 	template directory. If the underlying file system supports security on files and 
		/// 	directories, the function applies a specified security descriptor to the new directory. 
		/// 	The new directory retains the other attributes of the specified template directory.
		/// </summary>
		/// <param name = "lpTemplateDirectory">
		/// 	The path of the directory to use as a template 
		/// 	when creating the new directory. This parameter can be NULL.
		/// </param>
		/// <param name = "lpNewDirectory">The path of the directory to be created. </param>
		/// <param name = "lpSecurityAttributes">A pointer to a SECURITY_ATTRIBUTES structure. The lpSecurityDescriptor member of the structure specifies a security descriptor for the new directory.</param>
		/// <param name = "hTransaction">A handle to the transaction. This handle is returned by the CreateTransaction function.</param>
		/// <returns>True if the call succeeds, otherwise do a GetLastError.</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool CreateDirectoryTransactedW([MarshalAs(UnmanagedType.LPWStr)] string lpTemplateDirectory,
															   [MarshalAs(UnmanagedType.LPWStr)] string lpNewDirectory,
															   IntPtr lpSecurityAttributes,
															   SafeKernelTransactionHandle hTransaction);

		/// <summary>
		/// 	http://msdn.microsoft.com/en-us/library/aa365490(VS.85).aspx
		/// 	Deletes an existing empty directory as a transacted operation.
		/// </summary>
		/// <param name = "lpPathName">
		/// 	The path of the directory to be removed. 
		/// 	The path must specify an empty directory, 
		/// 	and the calling process must have delete access to the directory.
		/// </param>
		/// <param name = "hTransaction">A handle to the transaction. This handle is returned by the CreateTransaction function.</param>
		/// <returns>True if the call succeeds, otherwise do a GetLastError.</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool RemoveDirectoryTransactedW([MarshalAs(UnmanagedType.LPWStr)] string lpPathName,
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


		#endregion


	}
}