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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Transactions;
using Castle.Transactions.Contracts;

namespace Castle.Transactions
{
	using System.ComponentModel;

	using Castle.Transactions.Internal;

	/// <summary>
	/// 	<para>
	/// 		Denotes a castle transaction. This is the main point of interaction between your code and
	/// 		the transactional behaviour of it. Use the transaction manager <see cref = "ITransactionManager" /> to
	/// 		rollback from within a transactional method.
	/// 	</para><para>
	/// 	       	Implementors of this class should do their best to provide a stable implementation
	/// 	       	where Dispose, Rollback and Complete can be called idempotently. The get-property accessors must
	/// 	       	not change state when gotten.</para>
	/// </summary>
	[ContractClass(typeof (ITransactionContract))]
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// 	Dispose the resource/the transaction. It's important that you call this method
		/// 	when you are using the transaction together with the transaction manager, but 
		/// 	otherwise as well if you want deterministic disposal.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
			Justification = "I want to prove invariants about the disposed state, so I re-implement this method.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		new void Dispose();

		/// <summary>
		/// 	Gets the tranaction state. Castle.Service.Transaction contains a number
		/// 	of states which will allow you to reasin about the state.
		/// </summary>
		TransactionState State { get; }

		/// <summary>
		/// 	Gets the options used to create this transaction.
		/// </summary>
		ITransactionOptions CreationOptions { get; }

		/// <summary>
		/// 	<para>Gets the inner <see cref = "System.Transactions.Transaction" />,
		/// 		which is the foundation upon which Castle.Transactions builds.
		/// 		It can be either a <see cref = "CommittableTransaction" /> or a 
		/// 		<see cref = "DependentTransaction" /> or a 
		/// 		<see cref = "SubordinateTransaction" />. A dependent transaction
		/// 		can be used to handle concurrency in a nice way.</para>
		/// 
		/// 	<para>This property is null if the transaction's supervising coordinator (i.e.
		/// 		either MS DTC [multiple resources/2PC] or KTM [kernel/2PC] or LTM on Windows)
		/// 		is not based on LTM -- this is true (and hence the property null) for Kernel Transactions, i.e. registry
		/// 		and file transactions that were started before other DTC/LTM-transacted resources.</para>
		/// </summary>
		System.Transactions.Transaction Inner { get; }

		///// <summary>
		///// 	If the created transaction is a file transaction, there should be a
		///// 	transacted-file-transaction handle available.
		///// </summary>
		//Maybe<SafeKernelTransactionHandle> KernelTransactionHandle { get; }

		/// <summary>
		/// 	Gets a local identifier unique to the underlying transaction. Contrary to the 
		/// 	underlying System.Transactions.Transaction.TransactionInformation.LocalIdentifier
		/// 	property, this identifier is unique also across committable/dependent transactions
		/// 	whereas the former isn't. Hence, this identifier is well suited to implement
		/// 	per-transaction resolve semantics where even a dependent transaction requires a new 'context'
		/// 	of resolve.
		/// </summary>
		/// <remarks>
		/// As opposed to the local identifier on System.Transactions.Transaction.Current.LocalIdentifier
		/// this getter does not have side-effects and will return a value despite the inner transaction
		/// being disposed.
		/// </remarks>
		string LocalIdentifier { get; }

		/// <summary>
		/// 	Rolls the transaction back. This method is automatically called on (managed) dispose.
		/// </summary>
		void Rollback();

		/// <summary>
		/// 	Completes the transaction. This method can only be called if the 
		/// 	transaction is in the active state, i.e. begin has been called.
		/// </summary>
		/// <exception cref = "TransactionInDoubtException">
		/// 	The exception that is thrown when an operation 
		/// 	is attempted on a transaction that is in doubt, 
		/// 	or an attempt is made to commit the transaction 
		/// 	and the transaction becomes InDoubt. 
		/// </exception>
		/// <exception cref = "TransactionAbortedException">
		/// 	The exception that is thrown when an operation is attempted on a transaction 
		/// 	that has already been rolled back, or an attempt is made to commit 
		/// 	the transaction and the transaction aborts.
		/// </exception>
		/// <exception cref = "TransactionException">An unknown problem occurred. 
		/// 	For example the connection to the database was lost.</exception>
		/// <exception cref="AggregateException">
		///		One or more dependent transactions failed when using the Fork=true option!
		/// </exception>
		/// <remarks>
		/// 	It's up for grabs (i.e. github pull request) to correctly handle state on the two exceptions that may be thrown
		/// 	and to implement sane retry logic for them. All I can guess is that this shouldn't happen
		/// 	unless you run distributed transactions.
		/// </remarks>
		void Complete();
	}
}