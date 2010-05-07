using System;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// This interface shows that the transaction of transaction manager implementing
	/// it is aware of what is success (the completed event), failure or roll-backs.
	/// </summary>
	public interface IEventPublisher
	{
		/// <summary>
		/// Raised when the transaction rolled back successfully.
		/// </summary>
		event EventHandler<TransactionEventArgs> TransactionRolledBack;

		/// <summary>
		/// Raised when the transaction committed successfully.
		/// </summary>
		event EventHandler<TransactionEventArgs> TransactionCompleted;

		/// <summary>
		/// Raised when the transaction has failed on commit/rollback
		/// </summary>
		event EventHandler<TransactionFailedEventArgs> TransactionFailed;
	}
}