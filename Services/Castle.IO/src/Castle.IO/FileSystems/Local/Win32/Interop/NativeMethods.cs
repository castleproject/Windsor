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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	public static class NativeMethods
	{
		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedMember.Local

		internal const int ERROR_FILE_NOT_FOUND = 0x2;
		internal const int ERROR_PATH_NOT_FOUND = 0x3;
		internal const int ERROR_ACCESS_DENIED = 0x5;
		internal const int ERROR_INVALID_DRIVE = 0xf;
		internal const int ERROR_NO_MORE_FILES = 0x12;
		internal const int ERROR_INVALID_NAME = 0x7B;
		internal const int ERROR_ALREADY_EXISTS = 0xB7;
		internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE; // filename too long.
		internal const int ERROR_DIRECTORY = 0x10B;
		internal const int ERROR_OPERATION_ABORTED = 0x3e3;
		internal const int INVALID_FILE_ATTRIBUTES = -1;

		internal const int MAX_PATH = 260;
		// While Windows allows larger paths up to a maxium of 32767 characters, because this is only an approximation and
		// can vary across systems and OS versions, we choose a limit well under so that we can give a consistent behavior.
		internal const int MAX_LONG_PATH = 32000;
		internal const int MAX_ALTERNATE = 14;

		internal const string LongPathPrefix = @"\\?\";

		internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
		internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
		internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;

		internal static int MakeHRFromErrorCode(int errorCode)
		{
			return unchecked((int) 0x80070000 | errorCode);
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CopyFile(string src, string dst, [MarshalAs(UnmanagedType.Bool)] bool failIfExists);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern SafeFindHandle FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

		/// <summary>
		/// 	Continues a file search from a previous call to the FindFirstFile or FindFirstFileEx function.
		/// 	If there is a transaction bound to the file enumeration handle, then the files that are returned are subject to transaction isolation rules.
		/// </summary>
		/// <remarks>
		/// 	http://msdn.microsoft.com/en-us/library/aa364428%28v=VS.85%29.aspx
		/// </remarks>
		/// <param name = "hFindFile">The search handle returned by a previous call to the FindFirstFile or FindFirstFileEx function.</param>
		/// <param name = "lpFindFileData">    A pointer to the WIN32_FIND_DATA structure that receives information about the found file or subdirectory.
		/// 	The structure can be used in subsequent calls to FindNextFile to indicate from which file to continue the search.
		/// </param>
		/// <returns>If the function succeeds, the return value is nonzero and the lpFindFileData parameter contains information about the next file or directory found.
		/// 	If the function fails, the return value is zero and the contents of lpFindFileData are indeterminate. To get extended error information, call the GetLastError function.
		/// 	If the function fails because no more matching files can be found, the GetLastError function returns ERROR_NO_MORE_FILES.</returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FindNextFile(SafeFindHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern uint GetFullPathName(string lpFileName, uint nBufferLength,
		                                            StringBuilder lpBuffer, IntPtr mustBeNull);


		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteFile(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteFileW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool RemoveDirectory(string lpPathName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CreateDirectory(string lpPathName,
		                                            IntPtr lpSecurityAttributes);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool MoveFile(string lpPathNameFrom, string lpPathNameTo);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern SafeFileHandle CreateFile(
			[In] string lpFileName,
			[In] NativeFileAccess dwDesiredAccess,
			[In] NativeFileShare dwShareMode,
			[In, Optional] IntPtr lpSecurityAttributes,
			[In] NativeFileMode dwCreationDisposition,
			[In] NativeFileOptions dwFlagsAndAttributes,
			[In, Optional] IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern FileAttributes GetFileAttributes(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId,
		                                         StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);

		// overview here: http://msdn.microsoft.com/en-us/library/aa964885(VS.85).aspx
		// helper: http://www.improve.dk/blog/2009/02/14/utilizing-transactional-ntfs-through-dotnet

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FindClose(IntPtr hFindFile);

		public delegate CopyProgressResult CopyProgressRoutine(
			long TotalFileSize,
			long TotalBytesTransferred,
			long StreamSize,
			long StreamBytesTransferred,
			uint dwStreamNumber,
			CopyProgressCallbackReason dwCallbackReason,
			SafeFileHandle hSourceFile,
			SafeFileHandle hDestinationFile,
			IntPtr lpData);
	}
}