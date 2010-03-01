using System;
using System.Runtime.InteropServices;

namespace Castle.Services.Transaction
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
	internal interface IKernelTransaction
	{
		/// <summary>
		/// Gets a safe transaction handle. If we instead use IntPtr we 
		/// might not release the transaction handle properly.
		/// </summary>
		/// <param name="handle"></param>
		void GetHandle([Out] out SafeTxHandle handle);
	}
}