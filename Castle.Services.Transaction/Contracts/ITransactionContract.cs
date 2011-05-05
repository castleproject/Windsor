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
using Castle.Services.Transaction.IO;

namespace Castle.Services.Transaction.Contracts
{
	[ContractClassFor(typeof (ITransaction))]
	internal abstract class ITransactionContract : ITransaction
	{
		string ITransaction.LocalIdentifier
		{
			[Pure]
			get
			{
				Contract.Requires(State != TransactionState.Disposed);
				Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
				throw new NotImplementedException();
			}
		}

		void ITransaction.Rollback()
		{
			Contract.Ensures(State == TransactionState.Aborted);
		}

		void ITransaction.Complete()
		{
			Contract.Requires(State == TransactionState.Active);
			// ->
			Contract.Ensures(State == TransactionState.CommittedOrCompleted
			                 || State == TransactionState.Aborted
			                 || State == TransactionState.InDoubt);

			Contract.EnsuresOnThrow<TransactionException>(
				State == TransactionState.Aborted);
		}

		void ITransaction.Dispose()
		{
			Contract.Requires(State == TransactionState.Active
			                  || State == TransactionState.Active
			                  || State == TransactionState.Aborted
			                  || State == TransactionState.InDoubt
			                  || State == TransactionState.CommittedOrCompleted);

			Contract.Ensures(State == TransactionState.Disposed);
		}

		public TransactionState State
		{
			get { return Contract.Result<TransactionState>(); }
		}

		public ITransactionOptions CreationOptions
		{
			get { throw new NotImplementedException(); }
		}

		public System.Transactions.Transaction Inner
		{
			get { throw new NotImplementedException(); }
		}

		Maybe<SafeKernelTransactionHandle> ITransaction.KernelTransactionHandle
		{
			get
			{
				Contract.Ensures(Contract.Result<Maybe<SafeKernelTransactionHandle>>() != null);
				throw new NotImplementedException();
			}
		}

		//Maybe<IRetryPolicy> ITransaction.FailedPolicy
		//{
		//    [Pure]
		//    get
		//    {
		//        var result = Contract.Result<Maybe<IRetryPolicy>>();
		//        Contract.Ensures(result != null);
		//        return result;
		//    }
		//}

		void IDisposable.Dispose()
		{
		}
	}
}