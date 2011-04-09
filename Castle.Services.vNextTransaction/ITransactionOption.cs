using System;
using System.Transactions;

namespace Castle.Services.vNextTransaction
{
	public interface ITransactionOption : IEquatable<ITransactionOption>
	{
		/// <summary>
		/// Gets the transaction isolation level.
		/// </summary>
		IsolationLevel IsolationLevel { get; }

		/// <summary>
		/// Gets the transaction mode.
		/// </summary>
		TransactionScopeOption TransactionMode { get; }
		
		/// <summary>
		/// Gets whether the transaction is read only.
		/// </summary>
		bool ReadOnly { get; }

		/// <summary>
		/// Gets the Timeout for this managed transaction. Beware that the timeout 
		/// for the transaction option is not the same as your database has specified.
		/// Often it's a good idea to let your database handle the transactions
		/// timing out and leaving this option to its max value. Your mileage may vary though.
		/// </summary>
		TimeSpan Timeout { get; }
	}
}