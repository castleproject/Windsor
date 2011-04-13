#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class Activity : MarshalByRefObject
	{
		private Guid id = Guid.NewGuid();
		private readonly Stack<ITransaction> _TransactionStack = new Stack<ITransaction>(2);

		/// <summary>
		/// Gets the current transaction, i.e. the topmost one.
		/// </summary>
		[Pure]
		public ITransaction CurrentTransaction
		{
			get { return _TransactionStack.Count == 0 ? null : _TransactionStack.Peek(); }
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_TransactionStack != null);
		}

		/// <summary>
		/// Push a transaction onto the stack of transactions.
		/// </summary>
		/// <param name="transaction"></param>
		public void Push(ITransaction transaction)
		{
			Contract.Requires(transaction != null);
			Contract.Ensures(Contract.Exists(_TransactionStack, x=>
				object.ReferenceEquals(x, transaction)));
			Contract.Ensures(object.ReferenceEquals(CurrentTransaction, transaction));

			_TransactionStack.Push(transaction);
		}

		/// <summary>
		/// Return the top-most transaction from the stack of transactions.
		/// </summary>
		/// <returns></returns>
		public ITransaction Pop()
		{
			Contract.Ensures(Contract.ForAll(_TransactionStack,
				x => !object.ReferenceEquals(x, Contract.Result<ITransaction>())));

			return _TransactionStack.Pop();
		}

		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			var activity = obj as Activity;
			if (activity == null) return false;
			return Equals(id, activity.id);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
}
