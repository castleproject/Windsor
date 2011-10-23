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
using System.Diagnostics.Contracts;
using Castle.Services.Transaction.Contracts;

namespace Castle.Services.Transaction
{
	[ContractClass(typeof (IRetryPolicyContract))]
	public interface IRetryPolicy
	{
		/// <summary>
		/// </summary>
		/// <param name = "thrownException"></param>
		/// <returns>whether another retry should be made</returns>
		bool Retry(Exception thrownException);

		/// <summary>
		/// 	Gets the number of times the retry method has been called
		/// </summary>
		ulong Failures { get; }
	}
}