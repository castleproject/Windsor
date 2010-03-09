using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Castle.Services.Transaction
{
	///<summary>
	/// A safe file handle on the transaction resource.
	///</summary>    
	
	[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public sealed class SafeTxHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		/// Default c'tor
		/// </summary>
		public SafeTxHandle() : base(true)
		{
		}

		///<summary>
		/// c'tor taking a pointer to a transaction.
		///</summary>
		///<param name="handle">The transactional handle.</param>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public SafeTxHandle(IntPtr handle)
			: base(true)
		{
			base.handle = handle;
		}

		protected override bool ReleaseHandle()
		{
			if (!(IsInvalid || IsClosed))
			{
				return CloseHandle(handle);
			}
			return (IsInvalid || IsClosed);
		}

		/* BOOL WINAPI CloseHandle(
		 *		__in  HANDLE hObject
		 * );
		 */
		[DllImport("kernel32")]
		//[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		//[SuppressUnmanagedCodeSecurity]
		private static extern bool CloseHandle(IntPtr handle);
	}
}