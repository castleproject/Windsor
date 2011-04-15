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
	/// 	Specifies a method as transactional.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class TransactionAttribute : Attribute, ITransactionOption
	{
		private readonly IsolationLevel _IsolationLevel = IsolationLevel.ReadCommitted;
		private readonly TransactionScopeOption _TransactionMode;

		public TransactionAttribute() : this(TransactionScopeOption.Required)
		{
		}

		public TransactionAttribute(TransactionScopeOption transactionMode, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			Timeout = TimeSpan.MaxValue;
			_TransactionMode = transactionMode;
			_IsolationLevel = isolationLevel;
		}

		[Pure]
		IsolationLevel ITransactionOption.IsolationLevel
		{
			get { return _IsolationLevel; }
		}

		[Pure]
		TransactionScopeOption ITransactionOption.TransactionMode
		{
			get { return _TransactionMode; }
		}

		/// <summary>
		/// Read-only transactions cannot modify data.
		/// </summary>
		public bool ReadOnly { [Pure] get; set; }

		/// <summary>
		/// Gets or sets the transaction timeout. The timeout is often better
		/// implemented in the database, so this value is by default <see cref="TimeSpan.MaxValue"/>.
		/// </summary>
		public TimeSpan Timeout { [Pure] get; set; }

		/// <summary>
		/// 	Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// 	true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name = "other">An object to compare with this object.</param>
		[Pure]
		public bool Equals(ITransactionOption other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return base.Equals(other) && other.Timeout.Equals(Timeout) && Equals(other.IsolationLevel, _IsolationLevel) &&
			       Equals(other.TransactionMode, _TransactionMode) && other.ReadOnly.Equals(ReadOnly);
		}

		/// <summary>
		/// 	Returns a value that indicates whether this instance is equal to a specified object.
		/// </summary>
		/// <returns>
		/// 	true if <paramref name = "obj" /> equals the type and value of this instance; otherwise, false.
		/// </returns>
		/// <param name = "obj">An <see cref = "T:System.Object" /> to compare with this instance or null. </param>
		/// <filterpriority>2</filterpriority>
		[Pure]
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return Equals(obj as ITransactionOption);
		}

		/// <summary>
		/// 	Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// 	A 32-bit signed integer hash code.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				var result = base.GetHashCode();
				result = (result*397) ^ Timeout.GetHashCode();
				result = (result*397) ^ _IsolationLevel.GetHashCode();
				result = (result*397) ^ _TransactionMode.GetHashCode();
				result = (result*397) ^ ReadOnly.GetHashCode();
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