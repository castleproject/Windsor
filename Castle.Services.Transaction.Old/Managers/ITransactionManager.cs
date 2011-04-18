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

namespace Castle.Services.Transaction
{
	using System;

	/// <summary>
	/// Manages the creation and disposal of <see cref="ITransaction"/> instances.
	/// </summary>
	public interface ITransactionManager : IEventPublisher
	{
		/// <summary>
		/// Raised when a top level transaction was created
		/// </summary>
		event EventHandler<TransactionEventArgs> TransactionCreated;

		/// <summary>
		/// Raised when a child transaction was created
		/// </summary>
		event EventHandler<TransactionEventArgs> ChildTransactionCreated;

		/// <summary>
		/// Raised when the transaction was disposed
		/// </summary>
		event EventHandler<TransactionEventArgs> TransactionDisposed;
		
		/// <summary>
		/// <see cref="CreateTransaction(Castle.Services.Transaction.TransactionMode,Castle.Services.Transaction.IsolationMode,bool,bool)"/>.
		/// </summary>
		ITransaction CreateTransaction(TransactionMode transactionMode, IsolationMode isolationMode);

		/// <summary>
		/// Creates a transaction.
		/// </summary>
		/// <param name="transactionMode">The transaction mode.</param>
		/// <param name="isolationMode">The isolation mode.</param>
		/// <param name="isAmbient">if set to <c>true</c>, the TM will create a distributed transaction.</param>
		/// <param name="isReadOnly">if set to <c>true</c>, the TM will create a read only transaction.</param>
		/// <returns>
		/// null &lt;- If transactions are just supported, but there is no ambient transaction
		/// null &lt;- If transactions are not supported and there indeed is no ambient transaction (if there is, see exception docs)
		/// 
		/// </returns>
		/// <exception cref="TransactionModeUnsupportedException">
		/// transactionMode = <see cref="TransactionMode.NotSupported"/>
		/// and yet there is an ambient transaction in the transaction manager
		/// which is active.
		/// </exception>
		ITransaction CreateTransaction(TransactionMode transactionMode, IsolationMode isolationMode, bool isAmbient, bool isReadOnly);

		/// <summary>
		/// Returns the current <see cref="ITransaction"/>. 
		/// The transaction manager will probably need to 
		/// hold the created transaction in the thread or in 
		/// some sort of context.
		/// </summary>
		ITransaction CurrentTransaction { get; }

		/// <summary>
		/// Dispose the transaction passed appropriately, removing it from the list of tracked
		/// transactions, calling its dispose method and raise the <see cref="ITransactionManager.TransactionDisposed"/>
		/// event.
		/// </summary>
		/// <param name="transaction">The transaction to dispose</param>
		/// <exception cref="ArgumentNullException">transaction is null</exception>
		void Dispose(ITransaction transaction);
	}
}
