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
using System.Transactions;

namespace Castle.Services.vNextTransaction
{
	[ContractClassFor(typeof (ITransactionOptions))]
	internal abstract class ITransactionOptionsContract : ITransactionOptions
	{
		bool IEquatable<ITransactionOptions>.Equals(ITransactionOptions other)
		{
			throw new NotImplementedException();
		}

		IsolationLevel ITransactionOptions.IsolationLevel
		{
			get { throw new NotImplementedException(); }
		}

		TransactionScopeOption ITransactionOptions.Mode
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.ReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.Fork
		{
			get
			{
				Contract.Ensures(Contract.Result<bool>() || !((ITransactionOptions) this).WaitAll,
				                 "this would make no sense as true if Fork = false");
				throw new NotImplementedException();
			}
		}

		TimeSpan ITransactionOptions.Timeout
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.WaitAll
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.AsyncCommit
		{
			get
			{
				Contract.Ensures(Contract.Result<bool>() || !((ITransactionOptions) this).WaitAll,
				                 "this would make no sense as true if AsyncCommit = false (if you wait, it's not async!)");
				throw new NotImplementedException();
			}
		}
	}
}