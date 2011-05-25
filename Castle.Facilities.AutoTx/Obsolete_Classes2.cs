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
using System.Runtime.Serialization;
using System.Transactions;

namespace Castle.Facilities.Transactions
{
	[Obsolete("Deprecated; just remove this attribute for the same effect. No other changes are necessary.")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TransactionalAttribute : Attribute {}

	/// <summary>Obsolete. Remove usages. This attribute does nothing anymore.</summary>
	[Obsolete("Deprecated, use System.Transactions.TransactionScopeOption enum, instead.")]
	public enum TransactionMode
	{
		/// <summary>use "Requires" on TransactionScopeOption instead.</summary>
		Unspecified,

		/// <summary>use "Supress" on TransactionScopeOption instead.</summary>
		NotSupported,

		/// <summary>use "Requires" on TransactionScopeOption instead.</summary>
		Requires,

		/// <summary>use "RequiresNew" on TransactionScopeOption instead.</summary>
		RequiresNew,

		/// <summary>use "Requires" on TransactionScopeOption instead.</summary>
		Supported
	}

	/// <summary>Obsolete. Remove usages. Use <see cref="IsolationLevel"/> instead.</summary>
	[Obsolete("Remove usages. Use System.Transactions.IsolationLevel instead.")]
	public enum IsolationMode
	{
		Unspecified,
		Chaos,
		ReadCommitted,
		ReadUncommitted,
		RepeatableRead,
		Serializable
	}

	/// <summary>Obsolete. Remove usages. Use code like this instead:
	/// <code>using (var tx = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
	///{
	///	tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
	///	tx.Complete();
	///}</code>
	/// or alternatively resolve/reference in instance of <see cref="ITransactionManager"/> and use 
	/// <code>(:m).CurrentTransaction.Value.Inner.EnlistResource( <see cref="ISinglePhaseNotification"/> synchronization )</code> 
	/// instead.
	/// </summary>
	[Obsolete("Use ISinglePhaseNotification instead on the current inner (i.e. System.Transactions.Transaction) Transaction.")]
	public interface ISynchronization
	{
		/// <summary>Use <see cref="ISinglePhaseNotification"/> instead.</summary>
		void BeforeCompletion();

		/// <summary>Use <see cref="ISinglePhaseNotification"/> instead.</summary>
		void AfterCompletion();
	}

	/// <summary>
	/// These exceptions won't be thrown anymore. If you are using exceptions to validate your entities you should catch TransactionAbortedException instead of this.
	/// </summary>
	[Obsolete("These exceptions won't be thrown anymore. If you are using exceptions to validate your entities you should catch TransactionAbortedException instead of this.")]
	[Serializable]
	public class CommitResourceException : TransactionException
	{
		public CommitResourceException(string message) : base(message)
		{
		}

		public CommitResourceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public CommitResourceException()
		{
		}
	}
}