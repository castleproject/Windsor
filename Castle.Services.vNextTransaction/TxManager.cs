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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Castle.Services.vNextTransaction
{
	internal class TxManager : ITxManager, ITxManagerInternal
	{
		private readonly IActivityManager _ActivityManager;

		public TxManager(IActivityManager activityManager)
		{
			Contract.Requires(activityManager != null);
			Contract.Ensures(_ActivityManager != null);
			_ActivityManager = activityManager;
		}

		Maybe<ITransaction> ITxManager.CurrentTransaction
		{
			get { throw new NotImplementedException(); }
		}

		void ITxManager.AddRetryPolicy(string policyKey, Func<Exception, bool> retryPolicy)
		{
			throw new NotImplementedException();
		}

		void ITxManager.AddRetryPolicy(string policyKey, IRetryPolicy retryPolicy)
		{
			throw new NotImplementedException();
		}

		public Maybe<ITransaction> CreateTransaction(ITransactionOption transactionOption)
		{
			if (transactionOption.TransactionMode == TransactionScopeOption.Suppress)
				return Maybe.None<ITransaction>();

			var inner = new CommittableTransaction(new TransactionOptions
			                                       	{
			                                       		IsolationLevel = transactionOption.IsolationLevel,
			                                       		Timeout = TimeSpan.MaxValue
			                                       	});


			ITransaction tx = new Transaction(inner);
			var maybe = Maybe.Some(tx);
			Contract.Assume(maybe.HasValue && maybe.Value.State == TransactionState.Active);
			return maybe;
		}

		void IDisposable.Dispose()
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Abstracts approaches to keep transaction activities
	/// that may differ based on the environments.
	/// </summary>
	public interface IActivityManager
	{
		/// <summary>
		/// Gets the current activity.
		/// </summary>
		/// <value>The current activity.</value>
		Activity GetCurrentActivity();
	}

	/// <summary>
	/// Value-object that encapsulates a transaction and is serializable across
	/// app-domains.
	/// </summary>
	[Serializable]
	public sealed class Activity : MarshalByRefObject, IEquatable<Activity>
	{
		private readonly Guid _ActivityId = Guid.NewGuid();
		private readonly Stack<ITransaction> _Txs = new Stack<ITransaction>();

		public Maybe<ITransaction> CurrentTransaction
		{
			get { return _Txs.Count == 0 ? Maybe.None<ITransaction>() : Maybe.Some(_Txs.Peek()); }
		}

		public bool Equals(Activity other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other._ActivityId.Equals(_ActivityId);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Activity)) return false;
			return Equals((Activity) obj);
		}

		public override int GetHashCode()
		{
			return _ActivityId.GetHashCode();
		}

		public static bool operator ==(Activity left, Activity right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Activity left, Activity right)
		{
			return !Equals(left, right);
		}
	}
}