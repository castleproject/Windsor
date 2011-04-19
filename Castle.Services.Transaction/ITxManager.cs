#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// The transaction manager takes care of the nitty-gritty details of managing the store for transactions and their data.
	/// Its main use-case is creating the actual transactions, given the options for the transaction and the 
	/// be the place-to-go-to for knowing what transactions are currently ambient on the current call context.
	/// </summary>
	[ContractClass(typeof (ITxManagerContract))]
	public interface ITxManager : IDisposable
	{
		/// <summary>
		/// <para>Gets the current transaction. If the program has a call context
		/// located any methods further down the call-stack with methods with TransactionAttribute,
		/// this property gets the top most transaction which is the parent of the CurrentTransaction.
		/// </para>
		/// <para>
		/// Be aware that, when you call this property, only reads on pure properties on the transaction are thread-safe
		/// and no methods that are not static are thread-safe. This property can be used with good results to get 
		/// a transaction which you can use to register top-most resources in, such as rollback-aware NHibernate-session
		/// managers which can refresh the session if there's a fault.
		/// </para>
		/// <para>
		/// The value is Maybe.None() if there's no current transaction.
		/// </para>
		/// </summary>
		Maybe<ITransaction> CurrentTopTransaction { get; }

		/// <summary>
		/// <para>Gets the current transaction.
		/// The value is Maybe.None() if no transaction is on the current call context.</para>
		/// <para>
		/// If the current method has TransactionScopeOption.Supress specified but is inside a current transaction, then
		/// <see cref="System.Transactions.Transaction.Current"/> is null, but this property has the actual value of the transaction.</para>
		/// </summary>
		Maybe<ITransaction> CurrentTransaction { get; }

		/// <summary>
		/// Gets the number of transactions on the current context (in which calls to this
		/// interface is relevant).
		/// </summary>
		uint Count { get; }

		/// <summary>
		/// 	Add a new retry policy given a key and a function to execute.
		/// </summary>
		/// <param name = "policyKey"></param>
		/// <param name = "retryPolicy"></param>
		void AddRetryPolicy(string policyKey, Func<Exception, bool> retryPolicy);

		/// <summary>
		/// 	Add a new default retry policy based on key. It will be placed at the end of the chain.
		/// </summary>
		/// <param name = "policyKey"></param>
		/// <param name = "retryPolicy"></param>
		void AddRetryPolicy(string policyKey, IRetryPolicy retryPolicy);

		/// <summary>
		/// 	Create a new transaction, given the transaction options.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="IDisposable.Dispose"/> the transaction, or transactions further ahead in time will not
		/// work properly.
		/// </para><para>
		/// Also, beware that if you call this method on your own, you are responsible for setting
		/// <see cref="System.Transactions.Transaction.Current"/> to the result's Inner property
		/// and restoring the previous Current property at the end of that transaction.
		/// </para><para>
		/// The transaction interceptor (in AutoTx) takes care of this for you. The two projects
		/// work very well together.
		/// </para>
		/// </remarks>
		/// <param name = "transactionOptions">Options to use for creating the new transaction.</param>
		/// <returns>Maybe a transaction, if the options specified it.</returns>
		Maybe<ICreatedTransaction> CreateTransaction(ITransactionOptions transactionOptions);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="transactionOptions"></param>
		/// <returns></returns>
		Maybe<ICreatedTransaction> CreateFileTransaction(ITransactionOptions transactionOptions);
	}
}