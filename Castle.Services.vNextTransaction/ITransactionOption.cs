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
using System.Transactions;

namespace Castle.Services.vNextTransaction
{
	public interface ITransactionOption : IEquatable<ITransactionOption>
	{
		/// <summary>
		/// 	Gets the transaction isolation level.
		/// </summary>
		IsolationLevel IsolationLevel { get; }

		/// <summary>
		/// 	Gets the transaction mode.
		/// </summary>
		TransactionScopeOption TransactionMode { get; }

		/// <summary>
		/// 	Gets whether the transaction is read only.
		/// </summary>
		bool ReadOnly { get; }

		/// <summary>
		/// Gets whether the current transaction's method should forked off.
		/// </summary>
		bool Fork { get; }

		/// <summary>
		/// 	Gets the Timeout for this managed transaction. Beware that the timeout 
		/// 	for the transaction option is not the same as your database has specified.
		/// 	Often it's a good idea to let your database handle the transactions
		/// 	timing out and leaving this option to its max value. Your mileage may vary though.
		/// </summary>
		TimeSpan Timeout { get; }
	}
}