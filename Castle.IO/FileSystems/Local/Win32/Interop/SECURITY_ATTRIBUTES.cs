using System;
using System.Runtime.InteropServices;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	///<summary>
	///	Attributes for security interop.
	///</summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct SECURITY_ATTRIBUTES
	{
		public int nLength;
		public IntPtr lpSecurityDescriptor;
		public int bInheritHandle;
	}
}