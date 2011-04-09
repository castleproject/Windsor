#region license

// // Copyright 2009-2011 Henrik Feldt - http://logibit.se /
// // 
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// // 
// //     http://www.apache.org/licenses/LICENSE-2.0
// // 
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Castle.Services.vNextTransaction
{
	[ContractClass(typeof (TransactionContract))]
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// 	Gets information about the current transaction.
		/// </summary>
		TransactionInformation TransactionInformation { get; }

		/// <summary>
		/// 	Gets the failed policy, or null otherwise.
		/// </summary>
		Maybe<IRetryPolicy> FailedPolicy { get; }

		/// <summary>
		/// Rolls the transaction back.
		/// This method is automatically called on managed dispose.
		/// This method is idempotent.
		/// </summary>
		void Rollback();

		/// <summary>
		/// Completes the transaction. This method call is a nop if Rollback
		/// has already been called and is idempotent.
		/// </summary>
		void Complete();
	}

	[ContractClassFor(typeof (ITransaction))]
	internal abstract class TransactionContract : ITransaction
	{
		void IDisposable.Dispose()
		{
		}

		TransactionInformation ITransaction.TransactionInformation
		{
			get
			{
				var res = Contract.Result<TransactionInformation>();
				Contract.Ensures(Contract.Result<TransactionInformation>() != null);
				return res;
			}
		}

		Maybe<IRetryPolicy> ITransaction.FailedPolicy
		{
			get
			{
				var result = Contract.Result<Maybe<IRetryPolicy>>();
				Contract.Ensures(result != null);
				return result;
			}
		}

		void ITransaction.Rollback()
		{
			throw new NotImplementedException();
		}

		void ITransaction.Complete()
		{
			throw new NotImplementedException();
		}
	}
}