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
	public sealed class Transaction : ITransaction
	{
		private readonly CommittableTransaction _Inner;

		public Transaction(CommittableTransaction inner)
		{
			Contract.Requires(inner != null);
			Contract.Ensures(_Inner != null);
			_Inner = inner;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Inner != null, "the inner transaction mustn't be null");
		}

		internal bool HasRolledBack { get; private set; }

		public static ITransaction Current
		{
			get { throw new NotImplementedException(); }
		}

		TransactionInformation ITransaction.TransactionInformation
		{
			get { return _Inner.TransactionInformation; }
		}

		Maybe<IRetryPolicy> ITransaction.FailedPolicy
		{
			get { throw new NotImplementedException(); }
		}

		void ITransaction.Rollback()
		{
			HasRolledBack = true;
		}

		void ITransaction.Complete()
		{
			_Inner.Commit();
		}

		void IDisposable.Dispose()
		{
			_Inner.Dispose();
		}
	}
}