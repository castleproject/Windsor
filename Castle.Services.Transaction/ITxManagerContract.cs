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
using System.Transactions;

namespace Castle.Services.Transaction
{
	[ContractClassFor(typeof (ITxManager))]
	internal abstract class ITxManagerContract : ITxManager
	{
		public Maybe<ITransaction> CurrentTopTransaction
		{
			get
			{
				Contract.Ensures(Contract.Result<Maybe<ITransaction>>() != null);
				throw new NotImplementedException();
			}
		}


		public Maybe<ITransaction> CurrentTransaction
		{
			get
			{
				Contract.Ensures(Contract.Result<Maybe<ITransaction>>() != null);
				throw new NotImplementedException();
			}
		}

		public uint Count
		{
			get { throw new NotImplementedException(); }
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

		public Maybe<ICreatedTransaction> CreateTransaction()
		{
			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>() != null
			                 && (!Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			                     ||
			                     Contract.Result<Maybe<ICreatedTransaction>>().Value.Transaction.State == TransactionState.Active));

			throw new NotImplementedException();
		}

		public Maybe<ICreatedTransaction> CreateTransaction(ITransactionOptions transactionOptions)
		{
			Contract.Requires(transactionOptions != null);

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>() != null
			                 && (!Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			                     ||
			                     Contract.Result<Maybe<ICreatedTransaction>>().Value.Transaction.State == TransactionState.Active));

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			                 || transactionOptions.Mode == TransactionScopeOption.Suppress);

			throw new NotImplementedException();
		}

		public Maybe<ICreatedTransaction> CreateFileTransaction(ITransactionOptions transactionOptions)
		{
			Contract.Requires(transactionOptions != null);

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>() != null
			                 && (!Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			                     ||
			                     Contract.Result<Maybe<ICreatedTransaction>>().Value.Transaction.State == TransactionState.Active));

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			                 || transactionOptions.Mode == TransactionScopeOption.Suppress);

			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}