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
	/// <summary>
	/// <para>
	/// Denotes a castle transaction. This is the main point of interaction between your code and
	/// the transactional behaviour of it. Use the transaction manager <see cref = "ITxManager" /> to
	/// rollback from within a transactional method.
	/// </para><para>
	/// Implementors of this class should do their best to provide a stable implementation
	/// where Dispose, Rollback and Complete can be called idempotently. The get-property accessors must
	/// not change state when gotten.</para>
	/// </summary>
	[ContractClass(typeof (TransactionContract))]
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// Dispose the resource/the transaction. It's important that you call this method
		/// when you are using the transaction together with the transaction manager, but 
		/// otherwise as well if you want deterministic disposal.
		/// </summary>
		new void Dispose();

		/// <summary>
		/// Gets the tranaction state. Castle.Service.Transaction contains a number
		/// of states which will allow you to reasin about the state.
		/// </summary>
		TransactionState State { get; }

		/// <summary>
		/// Gets the inner <see cref="System.Transactions.Transaction"/>,
		/// which is the foundation upon which Castle.Transactions builds.
		/// It can be either a <see cref="CommittableTransaction"/> or a 
		/// <see cref="DependentTransaction"/> or a 
		/// <see cref="SubordinateTransaction"/>. A dependent transaction
		/// can be used to handle concurrency in a nice way.
		/// </summary>
		System.Transactions.Transaction Inner { get; }

		/// <summary>
		/// Gets information about the current underlying transaction
		/// such as those found in System.Transaction on top of which
		/// this framework is built.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// If the transaction is disposed you cannot access its information.
		/// </exception>
		TransactionInformation TransactionInformation { get; }

		// TODO: Policy for handling in doubt transactions

		/// <summary>
		/// Maybe contains a failed policy for this transaction.
		/// </summary>
		Maybe<IRetryPolicy> FailedPolicy { get; }

		/// <summary>
		/// Rolls the transaction back. This method is automatically called on (managed) dispose.
		/// </summary>
		void Rollback();

		/// <summary>
		/// Completes the transaction. This method can only be called if the 
		/// transaction is in the active state, i.e. begin has been called.
		/// </summary>
		/// <exception cref="TransactionInDoubtException">
		/// The exception that is thrown when an operation 
		/// is attempted on a transaction that is in doubt, 
		/// or an attempt is made to commit the transaction 
		/// and the transaction becomes InDoubt. 
		/// </exception>
		/// <exception cref="TransactionAbortedException">
		/// The exception that is thrown when an operation is attempted on a transaction 
		/// that has already been rolled back, or an attempt is made to commit 
		/// the transaction and the transaction aborts.
		/// </exception>
		/// <exception cref="TransactionException">An unknown problem occurred. 
		/// For example the connection to the database was lost.</exception>
		/// <remarks>
		/// It's up for grabs (i.e. github pull request) to correctly handle state on the two exceptions that may be thrown
		/// and to implement sane retry logic for them. All I can guess is that this shouldn't happen
		/// unless you run distributed transactions.
		/// </remarks>
		void Complete();
	}

	[ContractClassFor(typeof (ITransaction))]
	internal abstract class TransactionContract : ITransaction
	{
		void ITransaction.Rollback()
		{
		}

		void ITransaction.Complete()
		{
			Contract.Requires(State == TransactionState.Active);
			// ->
			Contract.Ensures(State == TransactionState.CommittedOrCompleted
				|| State == TransactionState.Aborted
				|| State == TransactionState.InDoubt);

			Contract.EnsuresOnThrow<TransactionException>(
				State == TransactionState.Aborted);
		}

		void ITransaction.Dispose()
		{
			Contract.Requires(State == TransactionState.Active
				|| State == TransactionState.Active
				|| State == TransactionState.Aborted
				|| State == TransactionState.InDoubt);

			Contract.Ensures(State == TransactionState.Diposed);
		}

		public TransactionState State
		{
			get { return Contract.Result<TransactionState>(); }
		}

		public System.Transactions.Transaction Inner
		{
			get
			{
				Contract.Ensures(Contract.Result<System.Transactions.Transaction>() != null);
				throw new NotImplementedException();
			}
		}

		[Pure]
		TransactionInformation ITransaction.TransactionInformation
		{
			get
			{
				Contract.Ensures(Contract.Result<TransactionInformation>() != null);
				return Contract.Result<TransactionInformation>();
			}
		}

		[Pure]
		Maybe<IRetryPolicy> ITransaction.FailedPolicy
		{
			get
			{
				var result = Contract.Result<Maybe<IRetryPolicy>>();
				Contract.Ensures(result != null);
				return result;
			}
		}

		void IDisposable.Dispose()
		{
		}
	}
}