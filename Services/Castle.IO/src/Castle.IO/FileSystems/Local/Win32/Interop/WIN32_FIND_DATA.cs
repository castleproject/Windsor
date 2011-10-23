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

using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WIN32_FIND_DATA
	{
		public FileAttributes dwFileAttributes;
		public FILETIME ftCreationTime;
		public FILETIME ftLastAccessTime;
		public FILETIME ftLastWriteTime;
		public int nFileSizeHigh;
		public int nFileSizeLow;
		public int dwReserved0;
		public int dwReserved1;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH)] public string cFileName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_ALTERNATE)] public string cAlternate;
	}
}