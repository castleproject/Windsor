namespace Castle.Facilities.Transactions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Transactions;

	/// <summary>
	/// 	Sample implementation of ITransactionOptions. Use this if you are using <see cref = "ITransactionManager" /> directly.
	/// </summary>
	public class DefaultTransactionOptions : ITransactionOptions, IEquatable<DefaultTransactionOptions>
	{
		private readonly IDictionary<string, object> _CustomContext;

		#region Implementation of ITransactionOptions

		public IsolationLevel IsolationLevel { get; set; }

		public TransactionScopeOption Mode { get; set; }

		public bool Fork { get; set; }

		public TimeSpan Timeout { get; set; }

		public IEnumerable<KeyValuePair<string, object>> CustomContext
		{
			get { return _CustomContext; }
		}

		public bool AsyncRollback { get; set; }

		public bool AsyncCommit { get; set; }
		
		public DependentCloneOption DependentOption { get; set; }

		#endregion

		public DefaultTransactionOptions()
			: this(IsolationLevel.ReadCommitted, TransactionScopeOption.Required, false, TimeSpan.MaxValue, false, false)
		{
		}

		public DefaultTransactionOptions(IsolationLevel isolationLevel, TransactionScopeOption mode, bool fork,
		                                 TimeSpan timeout, bool asyncRollback, bool asyncCommit)
			: this(isolationLevel, mode, fork, timeout, new Dictionary<string, object>(), asyncRollback, asyncCommit, DependentCloneOption.BlockCommitUntilComplete)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
			Justification = "Due to code-contracts' rewrite in debug mode")]
		public DefaultTransactionOptions(IsolationLevel isolationLevel, TransactionScopeOption mode, bool fork,
		                                 TimeSpan timeout,
		                                 IEnumerable<KeyValuePair<string, object>> customContext, bool asyncRollback,
		                                 bool asyncCommit, DependentCloneOption dependentCloneOption)
		{
			Contract.Requires(customContext != null);

			IsolationLevel = isolationLevel;
			Mode = mode;
			Fork = fork;
			Timeout = timeout;
			_CustomContext = customContext.ToDictionary(x => x.Key, x => x.Value);
			AsyncRollback = asyncRollback;
			AsyncCommit = asyncCommit;
			DependentOption = dependentCloneOption;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_CustomContext != null);
		}

		/// <summary>
		/// 	Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// 	true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name = "other">An object to compare with this object.</param>
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
			       && other.AsyncCommit.Equals(AsyncCommit)
				   && other.DependentOption.Equals(DependentOption);
		}

		/// <summary>
		/// 	Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// 	true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name = "other">An object to compare with this object.</param>
		public bool Equals(DefaultTransactionOptions other)
		{
			return Equals(other as ITransactionOptions);
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
			unchecked
			{
				var result = IsolationLevel.GetHashCode();
				result = (result*397) ^ Mode.GetHashCode();
				result = (result*397) ^ Fork.GetHashCode();
				result = (result*397) ^ Timeout.GetHashCode();
				result = (result*397) ^ _CustomContext.GetHashCode();
				result = (result*397) ^ AsyncRollback.GetHashCode();
				result = (result*397) ^ AsyncCommit.GetHashCode();
				result = (result*397) ^ DependentOption.GetHashCode();
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
}