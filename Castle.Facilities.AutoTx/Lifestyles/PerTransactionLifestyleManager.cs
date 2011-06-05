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
using Castle.Transactions;

namespace Castle.Facilities.AutoTx.Lifestyles
{
	/// <summary>
	/// 	A lifestyle manager that resolves a fresh instance for every transaction. In my opinion, this 
	/// 	is the most semantically correct option of the two per-transaction lifestyle managers: it's possible
	/// 	to audit your code to verify that sub-sequent calls to services don't start new transactions on their own.
	/// 	With this lifestyle, code executing in other threads work as expected, as no instances are shared accross these
	/// 	threads (this refers to the Fork=true option on the TransactionAttribute).
	/// </summary>
	public class PerTransactionLifestyleManager : PerTransactionLifestyleManagerBase
	{
		public PerTransactionLifestyleManager(ITransactionManager manager)
			: base(manager)
		{
			Contract.Requires(manager != null);
		}

		#region Overrides of PerTransactionLifestyleManagerBase

		protected internal override Maybe<ITransaction> GetSemanticTransactionForLifetime()
		{
			if (_Disposed)
				throw new ObjectDisposedException("PerTransactionLifestyleManager",
				                                  "The lifestyle manager is disposed and cannot be used.");

			return _Manager.CurrentTransaction;
		}

		#endregion
	}
}