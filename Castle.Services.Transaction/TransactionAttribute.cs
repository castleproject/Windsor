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
using System.Linq;
using System.Transactions;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// 	Specifies a method as transactional.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class TransactionAttribute : Attribute, ITransactionOptions
	{
		private IDictionary<string, object> _CustomContext;

		public TransactionAttribute() : this(TransactionScopeOption.Required)
		{
		}

		public TransactionAttribute(TransactionScopeOption mode, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			Timeout = TimeSpan.MaxValue;
			Mode = mode;
			IsolationLevel = isolationLevel;
			_CustomContext = new Dictionary<string, object>();
		}


		public IsolationLevel IsolationLevel { [Pure] get; set; }

		public TransactionScopeOption Mode { [Pure] get; set; }

		/// <summary>
		/// 	Gets or sets the transaction timeout. The timeout is often better
		/// 	implemented in the database, so this value is by default <see cref = "TimeSpan.MaxValue" />.
		/// </summary>
		public TimeSpan Timeout { [Pure] get; set; }

		/// <summary>
		/// 	Gets or sets whether the current transaction should be forked off as a unit of work to the thread pool.
		/// </summary>
		public bool Fork { [Pure] get; set; }

		/// <summary>
		/// 	Gets or sets whether the commit should be done asynchronously. Default is false. If you have done a lot of work
		/// 	in the transaction, an asynchronous commit might be preferrable.
		/// </summary>
		public bool AsyncCommit { [Pure] get; set; }

		/// <summary>
		/// 	Whether to perform the rollback asynchronously. This means that a failed transaction cleans up asynchronously.
		/// </summary>
		public bool AsyncRollback { [Pure] get; set; }

		/// <summary>
		/// 	Hint: <see cref = "Dictionary{TKey,TValue}" /> implements the return type.
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> CustomContext
		{
			get { return _CustomContext; }
			set { _CustomContext = value.ToDictionary(x => x.Key, x => x.Value); }
		}

		/// <summary>
		/// 	Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// 	true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name = "other">An object to compare with this object.</param>
		public bool Equals(TransactionAttribute other)
		{
			return Equals(other as ITransactionOptions);
		}

		public bool Equals(ITransactionOptions other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Equals(other.IsolationLevel, IsolationLevel)
			       && Equals(other.Mode, Mode)
			       && other.Fork.Equals(Fork)
			       && other.Timeout.Equals(Timeout)
			       && other.CustomContext.All(x => _CustomContext.ContainsKey(x.Key) && _CustomContext[x.Key].Equals(x.Value))
			       && other.AsyncRollback.Equals(AsyncRollback)
			       && other.AsyncCommit.Equals(AsyncCommit);
		}

		/// <summary>
		/// 	Determines whether the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />.
		/// </summary>
		/// <returns>
		/// 	true if the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />; otherwise, false.
		/// </returns>
		/// <param name = "obj">The <see cref = "T:System.Object" /> to compare with the current <see cref = "T:System.Object" />. </param>
		/// <filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is ITransactionOptions)) return false;
			return Equals((ITransactionOptions) obj);
		}

		/// <summary>
		/// 	Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// 	A hash code for the current <see cref = "T:System.Object" />.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			ITransactionOptions self = this;
			unchecked
			{
				var result = IsolationLevel.GetHashCode();
				result = (result*397) ^ Mode.GetHashCode();
				result = (result*397) ^ self.Fork.GetHashCode();
				result = (result*397) ^ self.Timeout.GetHashCode();
				result = (result*397) ^ CustomContext.GetHashCode();
				result = (result*397) ^ AsyncRollback.GetHashCode();
				result = (result*397) ^ AsyncCommit.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(TransactionAttribute left, TransactionAttribute right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(TransactionAttribute left, TransactionAttribute right)
		{
			return !Equals(left, right);
		}
	}
}