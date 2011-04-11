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

namespace Castle.Services.vNextTransaction
{
	[ContractClass(typeof (TxManagerContract))]
	public interface ITxManager : IDisposable
	{
		/// <summary>
		/// 	Gets the current transaction. 
		/// 	The value is null if no transaction is on the current call context.
		/// </summary>
		Maybe<ITransaction> CurrentTransaction { get; }

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
		/// <param name = "transactionOption">Options to use for creating the new transaction.</param>
		/// <returns>Maybe a transaction, if the options specified it.</returns>
		Maybe<ITransaction> CreateTransaction(ITransactionOption transactionOption);
	}

	[ContractClassFor(typeof (ITxManager))]
	internal abstract class TxManagerContract : ITxManager
	{
		public Maybe<ITransaction> CurrentTransaction
		{
			get
			{
				var result = Contract.Result<Maybe<ITransaction>>();
				Contract.Ensures(result != null);
				return result;
			}
		}

		public void AddRetryPolicy(string policyKey, Func<Exception, bool> retryPolicy)
		{
			Contract.Requires(policyKey != null);
			Contract.Requires(retryPolicy != null);
		}

		public void AddRetryPolicy(string policyKey, IRetryPolicy retryPolicy)
		{
			Contract.Requires(policyKey != null);
			Contract.Requires(retryPolicy != null);
		}

		public Maybe<ITransaction> CreateTransaction(ITransactionOption transactionOption)
		{
			Contract.Requires(transactionOption != null);

			Contract.Ensures(Contract.Result<Maybe<ITransaction>>() != null
				&& (!Contract.Result<Maybe<ITransaction>>().HasValue
					|| Contract.Result<Maybe<ITransaction>>().Value.State == TransactionState.Active));
			
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}