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
using Castle.Transactions.Contracts;

namespace Castle.Transactions
{
	[ContractClass(typeof (ITransactionOptionsContract))]
	public interface ITransactionOptions : IEquatable<ITransactionOptions>
	{
		/// <summary>
		/// 	Gets the transaction isolation level.
		/// </summary>
		IsolationLevel IsolationLevel { [Pure] get; }

		/// <summary>
		/// 	Gets the transaction mode.
		/// </summary>
		TransactionScopeOption Mode { [Pure] get; }

		/// <summary>
		///		<para>Gets the dependent clone option, i.e. the option that
		///		specifies what to do with the child/dependent transaction
		///		if the parent transaction completes before. The default is
		///		BlockCommitUntilComplete and unless you know what you are doing, this is
		///		a recommended setting.</para>
		///		<para>
		///		If you are COMPLETELY SURE you want the main, non-forked transaction
		///		to complete without caring for its forked transactions (i.e. racing to complete
		///		with them an introducing the random execution patterns this brings), set this
		///		property to the non-default RollbackIfNotComplete. This will throw
		///		TransactionAbortedExceptions on all forked transactions, which will be silently dropped. 
		/// </para>
		/// </summary>
		DependentCloneOption DependentOption { [Pure] get; }

		/// <summary>
		/// 	Gets whether the current transaction's method should forked off. You might get deadlocks
		///		if you have only set one thread on the thread pool.
		/// </summary>
		bool Fork { [Pure] get; }

		/// <summary>
		/// 	Gets the Timeout for this managed transaction. Beware that the timeout 
		/// 	for the transaction option is not the same as your database has specified.
		/// 	Often it's a good idea to let your database handle the transactions
		/// 	timing out and leaving this option to its max value. Your mileage may vary though.
		/// </summary>
		TimeSpan Timeout { [Pure] get; }

		/// <summary>
		/// 	Version 3.1: Gets whether the commit should be done asynchronously. Default is false. If you have done a lot of work
		/// 	in the transaction, an asynchronous commit might be preferrable.
		/// </summary>
		bool AsyncCommit { [Pure] get; }

		/// <summary>
		/// 	Version 3.1: Gets whether a failed transaction should rollback asynchronously after notifying the caller of failure.
		/// </summary>
		bool AsyncRollback { [Pure] get; }
	}


	// <summary>
	// 	Gets whether the transaction is read only. The default implementations do not in any way honor this
	//		flag in any other way than passing it to the transaction to do what it wants with. Read-only transactions
	//		are mostly a concept in relational databases and are used as an optimization technique. Even if this flag is set
	//		the out-of-the-box implementation(s) will not in any way enforce it.
	// </summary>
}