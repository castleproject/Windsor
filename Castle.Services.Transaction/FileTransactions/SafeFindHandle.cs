using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Castle.Services.Transaction
{
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SafeFindHandle()
			: base(true)
		{
		}

		public SafeFindHandle(IntPtr preExistingHandle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(preExistingHandle);
		}

		protected override bool ReleaseHandle()
		{
			if (!(IsInvalid || IsClosed))
			{
				return FindClose(this);
			}
			return (IsInvalid || IsClosed);
		}

		protected override void Dispose(bool disposing)
		{
			if (!(IsInvalid || IsClosed))
			{
				FindClose(this);
			}
			base.Dispose(disposing);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool FindClose(SafeHandle hFindFile);
	}
}