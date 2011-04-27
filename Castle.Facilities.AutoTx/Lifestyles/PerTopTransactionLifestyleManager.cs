#region license

// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Lifestyles
{
	/// <summary>
	/// 	A lifestyle manager for every top transaction in the current call context. This lifestyle is great
	/// 	for components that are thread-safe and need to monitor/handle items in both the current thread
	/// 	and any forked method invocations. It's also favoring memory if your application is single threaded,
	/// 	as there's no need to create a new component every sub-transaction. (this refers to the Fork=true option
	/// 	on the TransactionAttribute).
	/// </summary>
	public class PerTopTransactionLifestyleManager : PerTransactionLifestyleManagerBase
	{
		public PerTopTransactionLifestyleManager(ITransactionManager manager)
			: base(manager)
		{
			Contract.Requires(manager != null);
		}

		#region Overrides of PerTransactionLifestyleManagerBase

		protected internal override Maybe<ITransaction> GetSemanticTransactionForLifetime()
		{
			if (_Disposed)
				throw new ObjectDisposedException("PerTopTransactionLifestyleManager",
				                                  "The lifestyle manager is disposed and cannot be used.");

			return _Manager.CurrentTopTransaction;
		}

		#endregion
	}
}