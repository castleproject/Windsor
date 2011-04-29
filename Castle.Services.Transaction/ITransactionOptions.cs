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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Transactions;
using Castle.Services.Transaction.Contracts;

namespace Castle.Services.Transaction
{
	[ContractClass(typeof (ITransactionOptionsContract))]
	public interface ITransactionOptions : IEquatable<ITransactionOptions>
	{
		/// <summary>
		/// 	Gets the transaction isolation level.
		/// </summary>
		IsolationLevel IsolationLevel { [Pure]
		get; }

		/// <summary>
		/// 	Gets the transaction mode.
		/// </summary>
		TransactionScopeOption Mode { [Pure]
		get; }

		/// <summary>
		/// 	Gets whether the current transaction's method should forked off.
		/// </summary>
		bool Fork { [Pure]
		get; }

		/// <summary>
		/// 	Gets the Timeout for this managed transaction. Beware that the timeout 
		/// 	for the transaction option is not the same as your database has specified.
		/// 	Often it's a good idea to let your database handle the transactions
		/// 	timing out and leaving this option to its max value. Your mileage may vary though.
		/// </summary>
		TimeSpan Timeout { [Pure]
		get; }

		/// <summary>
		/// 	Version 3.1: Gets whether the commit should be done asynchronously. Default is false. If you have done a lot of work
		/// 	in the transaction, an asynchronous commit might be preferrable.
		/// </summary>
		bool AsyncCommit { [Pure]
		get; }

		/// <summary>
		/// 	Version 3.1: Gets whether a failed transaction should rollback asynchronously after notifying the caller of failure.
		/// </summary>
		bool AsyncRollback { [Pure]
		get; }

		/// <summary>
		/// 	Gets the custom context dictionary. Implementors of the interface can choose to perform
		/// 	custom logic based on the items in this dictionary. For example, if your infrastructure
		/// 	is capable of handling Database ReadOnly Transactions, tell the infrastructure
		/// 	that through this context-property.
		/// </summary>
		IEnumerable<KeyValuePair<string, object>> CustomContext { [Pure]
		get; }
	}


	// <summary>
	// 	Gets whether the transaction is read only. The default implementations do not in any way honor this
	//		flag in any other way than passing it to the transaction to do what it wants with. Read-only transactions
	//		are mostly a concept in relational databases and are used as an optimization technique. Even if this flag is set
	//		the out-of-the-box implementation(s) will not in any way enforce it.
	// </summary>
}