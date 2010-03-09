// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Services.Transaction
{
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Represents the contract for a transaction.
	/// </summary>
	public interface ITransaction
	{
		/// <summary>
		/// Starts the transaction. Implementors
		/// should activate the apropriate resources
		/// in order to start the underlying transaction
		/// </summary>
		void Begin();

		/// <summary>
		/// Succeed the transaction, persisting the
		/// modifications
		/// </summary>
		void Commit();

		/// <summary>
		/// Cancels the transaction, rolling back the 
		/// modifications
		/// </summary>
		void Rollback();

		/// <summary>
		/// Signals that this transaction can only be rolledback. 
		/// This is used when the transaction is not being managed by
		/// the callee.
		/// </summary>
		void SetRollbackOnly();

		/// <summary>
		/// Returns the current transaction status.
		/// </summary>
		TransactionStatus Status { get; }
		
		/// <summary>
		/// Register a participant on the transaction.
		/// </summary>
		/// <param name="resource"></param>
		void Enlist(IResource resource);

		/// <summary>
		/// Registers a synchronization object that will be 
		/// invoked prior and after the transaction completion
		/// (commit or rollback)
		/// </summary>
		/// <param name="synchronization"></param>
		void RegisterSynchronization(ISynchronization synchronization);

		/// <summary>
		/// Transaction context. Can be used by applications.
		/// </summary>
		IDictionary Context { get; }

		/// <summary>
		/// Gets whether the transaction is running inside another of castle's transactions.
		/// </summary>
		bool IsChildTransaction { get; }

		/// <summary>
		/// Gets whether rollback only is set.
		/// </summary>
		bool IsRollbackOnlySet { get; }

		/// <summary>
		/// Gets the transaction mode of the transaction.
		/// </summary>
		TransactionMode TransactionMode { get; }

		/// <summary>
		/// Gets the isolation mode in use for the transaction.
		/// </summary>
		IsolationMode IsolationMode { get; }

		/// <summary>
		/// Gets whether the transaction "found an" ambient transaction to run in.
		/// This is true if the tx is running in the DTC or a TransactionScope, but 
		/// doesn't imply a distributed transaction (as TransactionScopes automatically choose the least
		/// performance invasive option)
		/// </summary>
		bool IsAmbientTransaction { get; }

		/// <summary>
		/// Gets the friendly name (if set) or an unfriendly integer hash name (if not set).
		/// Never returns null.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets an enumerable of the resources present.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IResource> Resources();
	}
}
