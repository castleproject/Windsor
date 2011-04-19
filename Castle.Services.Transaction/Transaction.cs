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
using TransactionException = Castle.Services.Transaction.Exceptions.TransactionException;

namespace Castle.Services.Transaction
{

	public enum TransactionState
	{
		/// <summary>Initial state before c'tor run</summary>
		Default,

		/// <summary>When begin has been called and has returned.</summary>
		Active,

		/// <summary>When the transaction is in doubt.</summary>
		InDoubt,

		/// <summary>When commit has been called and has returned successfully.</summary>
		CommittedOrCompleted,

		/// <summary>When first begin and then rollback has been called, or
		/// a resource failed.</summary>
		Aborted,

		/// <summary>When the dispose method has run.</summary>
		Diposed
	}

	[Serializable]
	public class Transaction : ITransaction
	{
		private TransactionState _State = TransactionState.Default;

		private uint _StackDepth;
		private readonly ITransactionOptions _CreationOptions;
		private readonly CommittableTransaction _Inner;
		private readonly DependentTransaction _Inner2;
		
		[NonSerialized]
		private readonly Action _OnDispose;

		public Transaction(CommittableTransaction inner, uint stackDepth, ITransactionOptions creationOptions, Action onDispose)
		{
			Contract.Requires(creationOptions != null);
			Contract.Requires(inner != null);
			Contract.Ensures(_Inner != null);
			Contract.Ensures(_State == TransactionState.Active);
			Contract.Ensures(((ITransaction)this).State == TransactionState.Active);
			_Inner = inner;
			_StackDepth = stackDepth;
			_CreationOptions = creationOptions;
			_OnDispose = onDispose;
			_State = TransactionState.Active;
		}

		public Transaction(DependentTransaction inner, uint stackDepth, ITransactionOptions creationOptions, Action onDispose)
		{
			Contract.Requires(creationOptions != null);
			Contract.Requires(inner != null);
			Contract.Ensures(_Inner2 != null);
			Contract.Ensures(_State == TransactionState.Active);
			Contract.Ensures(((ITransaction)this).State == TransactionState.Active);
			_Inner2 = inner;
			_StackDepth = stackDepth;
			_CreationOptions = creationOptions;
			_OnDispose = onDispose;
			_State = TransactionState.Active;
		}

		/**
		 * Possible state changes
		 * 
		 * Default -> Constructed
		 * Constructed -> Disposed
		 * Constructed -> Active
		 * Active -> CommittedOrCompleted (depends on whether we are committable or not)
		 * Active -> InDoubt
		 * Active -> Aborted
		 * Aborted -> Disposed	# an active transaction may be disposed and then dispose must take take of aborting
		 */

		void ITransaction.Dispose()
		{
			try
			{
				try
				{
					if (_State == TransactionState.Active)
						InnerRollback();
				}
				finally {
					if (_OnDispose != null) _OnDispose();
					((IDisposable) this).Dispose();
				}
			}
			finally
			{
				_State = TransactionState.Diposed;
			}
		}

		TransactionState ITransaction.State
		{
			get
			{
				Contract.Ensures(Contract.Result<TransactionState>() == _State);
				return _State;
			}
		}

		ITransactionOptions ITransaction.CreationOptions
		{
			get { return _CreationOptions; }
		}

		System.Transactions.Transaction ITransaction.Inner
		{
			get
			{
				Contract.Assume(_Inner != null || _Inner2 != null);
				return _Inner ?? (System.Transactions.Transaction) _Inner2;
			}
		}

		Maybe<SafeKernelTxHandle> ITransaction.TxFHandle
		{
			get { return Maybe.None<SafeKernelTxHandle>(); }
		}

		private System.Transactions.Transaction Inner
		{
			get { return _Inner ?? (System.Transactions.Transaction) _Inner2; }
		}

		Maybe<IRetryPolicy> ITransaction.FailedPolicy
		{
			get { return Maybe.None<IRetryPolicy>(); }
		}

		string ITransaction.LocalIdentifier
		{
			get
			{
				var s = Inner.TransactionInformation.LocalIdentifier + ":" + _StackDepth;
				Contract.Assume(!string.IsNullOrEmpty(s), "because string concatenation doesn't return null or empty if one part is a constant non-empty string");
				return s;
			}
		}

		void ITransaction.Rollback()
		{
			InnerRollback();
		}

		private void InnerRollback()
		{
			Contract.Ensures(_State == TransactionState.Aborted);

			try
			{
				Inner.Rollback();
			}
			finally
			{
				_State = TransactionState.Aborted;
			}
		}

		void ITransaction.Complete()
		{
			try
			{
				if (_Inner != null) _Inner.Commit();
				if (_Inner2 != null) _Inner2.Complete();

				_State = TransactionState.CommittedOrCompleted;
			}
			catch (TransactionInDoubtException e)
			{
				_State = TransactionState.InDoubt;
				throw new TransactionException("Transaction in doubt. See inner exception and help link for details",
											   e,
											   new Uri("http://support.microsoft.com/kb/899115/EN-US/"));
			}
			catch (TransactionAbortedException)
			{
				_State = TransactionState.Aborted;
				throw;
			}
			catch(Exception e)
			{
				InnerRollback();
				// we might have lost connection to the database
				// or any other myriad of problems might have occurred
				throw new TransactionException("unknown transaction problem, see inner exception", e,
				                               new Uri("http://stackoverflow.com/questions/224689/transactions-in-net"));
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isManaged)
		{
			if (!isManaged)
				return;

			try
			{
				Inner.Dispose();
			}
			finally
			{
				_State = TransactionState.Diposed;
			}
		}
	}
}