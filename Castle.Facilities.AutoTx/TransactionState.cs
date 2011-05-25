using System.Transactions;

namespace Castle.Facilities.Transactions
{
	/// <summary>
	/// An enum of the possible states a transaction might take. Due to
	/// the concurrent nature of a transaction that is talking to a resource manager
	/// hosted in another thread and the nature of distributed software, the transaction
	/// state is only a best guess (there is no truth), based on what methods have been
	/// called on the Transaction. If, for example, a <see cref="DependentTransaction"/>
	/// has Rollback() called on it, the parent transaction has no way of getting
	/// deterministically notified, so its state will still be active, until
	/// Commit() is called on it (or <see cref="ITransaction"/>.Complete() in the case of the API);
	/// then <see cref="TransactionAbortedException"/> will be thrown.
	/// </summary>
	public enum TransactionState
	{
		/// <summary>
		/// 	Initial state before c'tor run
		/// </summary>
		Default,

		/// <summary>
		/// 	When begin has been called and has returned.
		/// </summary>
		Active,

		/// <summary>
		/// 	When the transaction is in doubt. This occurs if e.g. the durable resource
		///		fails after Prepare but before the ACK for Commit has reached the application on
		///		which this transaction framework is running.
		/// </summary>
		InDoubt,

		/// <summary>
		/// 	When commit has been called and has returned successfully.
		/// </summary>
		CommittedOrCompleted,

		/// <summary>
		/// 	When first begin and then rollback has been called, or
		/// 	a resource failed.
		/// </summary>
		Aborted,

		/// <summary>
		/// 	When the dispose method has run.
		/// </summary>
		Disposed
	}
}