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
using System.Transactions;

namespace Castle.Services.vNextTransaction
{
	public class DefaultTransactionOptions : ITransactionOptions, IEquatable<DefaultTransactionOptions>
	{
		public DefaultTransactionOptions()
			: this(System.Transactions.IsolationLevel.ReadCommitted,
			TransactionScopeOption.Required,
			false,
			false,
			TimeSpan.MaxValue)
		{
		}

		public DefaultTransactionOptions(IsolationLevel isolationLevel,
			TransactionScopeOption mode,
			bool readOnly, 
			bool fork, 
			TimeSpan timeout)
		{
			IsolationLevel = isolationLevel;
			Mode = mode;
			ReadOnly = readOnly;
			Fork = fork;
			Timeout = timeout;
		}

		#region Implementation of ITransactionOptions

		public IsolationLevel IsolationLevel { get; private set; }

		public TransactionScopeOption Mode { get; private set; }

		public bool ReadOnly { get; private set; }

		public bool Fork { get; private set; }

		public TimeSpan Timeout { get; private set; }

		#endregion

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(DefaultTransactionOptions other)
		{
			return Equals((ITransactionOptions) other);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ITransactionOptions other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.IsolationLevel, IsolationLevel) && Equals(other.Mode, Mode) && other.ReadOnly.Equals(ReadOnly) && other.Fork.Equals(Fork) && other.Timeout.Equals(Timeout);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is ITransactionOptions)) return false;
			return Equals((ITransactionOptions) obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. 
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			unchecked
			{
				int result = IsolationLevel.GetHashCode();
				result = (result*397) ^ Mode.GetHashCode();
				result = (result*397) ^ ReadOnly.GetHashCode();
				result = (result*397) ^ Fork.GetHashCode();
				result = (result*397) ^ Timeout.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(DefaultTransactionOptions left, DefaultTransactionOptions right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DefaultTransactionOptions left, DefaultTransactionOptions right)
		{
			return !Equals(left, right);
		}
	}

	public interface ITransactionOptions : IEquatable<ITransactionOptions>
	{
		/// <summary>
		/// 	Gets the transaction isolation level.
		/// </summary>
		IsolationLevel IsolationLevel { get; }

		/// <summary>
		/// 	Gets the transaction mode.
		/// </summary>
		TransactionScopeOption Mode { get; }

		/// <summary>
		/// 	Gets whether the transaction is read only.
		/// </summary>
		bool ReadOnly { get; }

		/// <summary>
		/// Gets whether the current transaction's method should forked off.
		/// </summary>
		bool Fork { get; }

		/// <summary>
		/// 	Gets the Timeout for this managed transaction. Beware that the timeout 
		/// 	for the transaction option is not the same as your database has specified.
		/// 	Often it's a good idea to let your database handle the transactions
		/// 	timing out and leaving this option to its max value. Your mileage may vary though.
		/// </summary>
		TimeSpan Timeout { get; }
	}
}