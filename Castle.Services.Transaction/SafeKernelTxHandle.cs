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

using System.Security;
using Castle.Services.Transaction.Internal;

namespace Castle.Services.Transaction
{
	using System;
	using System.Runtime.ConstrainedExecution;
	using Microsoft.Win32.SafeHandles;

	///<summary>
	/// A safe file handle on the transaction resource.
	///</summary>    
	
	[SecurityCritical]
	public sealed class SafeKernelTxHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		/// Default c'tor
		/// </summary>
		public SafeKernelTxHandle() : base(true)
		{
		}

		///<summary>
		/// c'tor taking a pointer to a transaction.
		///</summary>
		///<param name="handle">The transactional handle.</param>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public SafeKernelTxHandle(IntPtr handle)
			: base(true)
		{
			base.handle = handle;
		}

		protected override bool ReleaseHandle()
		{
			if (!(IsInvalid || IsClosed))
			{
				return NativeMethods.CloseHandle(handle);
			}
			return (IsInvalid || IsClosed);
		}
	}
}