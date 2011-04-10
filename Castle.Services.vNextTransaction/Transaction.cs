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

	public enum TransactionState
	{
		/// <summary>Initial state before c'tor run</summary>
		Default,

		/// <summary>When the transaction constructor has run.</summary>
		Constructed,

		/// <summary>When begin has been called and has returned.</summary>
		Active,

		/// <summary>When the transaction is in doubt.</summary>
		InDoubt,

		/// <summary>When commit has been called and has returned successfully.</summary>
		Committed,

		/// <summary>When first begin and then rollback has been called.</summary>
		Aborted,

		/// <summary>When the dispose method has run.</summary>
		Diposed
	}
	public sealed class Transaction : ITransaction
	{
		private TransactionState _State = TransactionState.Default;

		private readonly CommittableTransaction _Inner;

		public Transaction(CommittableTransaction inner)
		{
			Contract.Requires(inner != null);
			Contract.Ensures(_Inner != null);
			Contract.Ensures(_State == TransactionState.Constructed);
			Contract.Ensures(((ITransaction)this).State == TransactionState.Constructed);
			_Inner = inner;
			_State = TransactionState.Constructed;
		}

		/**
		 * Possible state changes
		 * 
		 * Default -> Constructed
		 * Constructed -> Disposed
		 * Constructed -> Active
		 * Active -> Committed
		 * Active -> InDoubt
		 * Active -> Aborted
		 * Aborted -> Disposed	# an active transaction may be disposed and then dispose must take take of aborting
		 */

		public static ITransaction Current
		{
			get { throw new NotImplementedException(); }
		}

		void ITransaction.Dispose()
		{
			try
			{
				((IDisposable) this).Dispose();
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

		TransactionInformation ITransaction.TransactionInformation
		{
			get { return _Inner.TransactionInformation; }
		}

		Maybe<IRetryPolicy> ITransaction.FailedPolicy
		{
			get { throw new NotImplementedException(); }
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
				_Inner.Rollback();
			}
			finally
			{
				_State = TransactionState.Aborted;
			}
		}

		void ITransaction.Begin()
		{
			throw new NotImplementedException();
		}

		void ITransaction.Complete()
		{
			// TODO: Policy for handling in doubt transactions

			try
			{
				_Inner.Commit();
				_State = TransactionState.Committed;
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
			try
			{
				_Inner.Dispose();
			}
			finally
			{
				_State = TransactionState.Diposed;
			}
		}
	}
}