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

namespace Castle.Transactions.Contracts
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

		DependentCloneOption ITransactionOptions.DependentOption
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.Fork
		{
			get { throw new NotImplementedException(); }
		}

		TimeSpan ITransactionOptions.Timeout
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.AsyncCommit
		{
			get { throw new NotImplementedException(); }
		}

		bool ITransactionOptions.AsyncRollback
		{
			get { throw new NotImplementedException(); }
		}

		//IEnumerable<KeyValuePair<string, object>> ITransactionOptions.CustomContext
		//{
		//    get
		//    {
		//        Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<string, object>>>() != null);
		//        throw new NotImplementedException();
		//    }
		//}
	}
}