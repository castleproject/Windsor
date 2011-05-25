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

namespace Castle.Services.Transaction.Internal
{
	internal sealed class CreatedTransaction : ICreatedTransaction
	{
		private readonly ITransaction _Transaction;
		private readonly bool _ShouldFork;
		private readonly Func<IDisposable> _ForkScopeFactory;

		public CreatedTransaction(ITransaction transaction, bool shouldFork, Func<IDisposable> forkScopeFactory)
		{
			Contract.Requires(forkScopeFactory != null);
			Contract.Requires(transaction != null);
			Contract.Requires(transaction.State == TransactionState.Active);

			_Transaction = transaction;
			_ShouldFork = shouldFork;
			_ForkScopeFactory = forkScopeFactory;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Transaction != null);
			Contract.Invariant(_ForkScopeFactory != null);
		}

		#region Implementation of ICreatedTransaction

		ITransaction ICreatedTransaction.Transaction
		{
			get { return _Transaction; }
		}

		bool ICreatedTransaction.ShouldFork
		{
			get { return _ShouldFork; }
		}

		IDisposable ICreatedTransaction.GetForkScope()
		{
			var disposable = _ForkScopeFactory();

			if (disposable == null)
				throw new InvalidOperationException("fork scope factory returned null!");

			return disposable;
		}

		#endregion
	}
}