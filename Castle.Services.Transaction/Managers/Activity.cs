#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http:www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.Services.Transaction
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class Activity : MarshalByRefObject
	{
		private Guid id = Guid.NewGuid();
		private readonly Stack<ITransaction> _TransactionStack = new Stack<ITransaction>(2);

		public ITransaction CurrentTransaction
		{
			get
			{
				return _TransactionStack.Count == 0 ? null : _TransactionStack.Peek();
			}
		}

		public void Push(ITransaction transaction)
		{
			_TransactionStack.Push(transaction);
		}

		public ITransaction Pop()
		{
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
