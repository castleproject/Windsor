using Castle.Transactions.Activities;

namespace Castle.Transactions
{
	/// <summary>
	/// 	Abstracts approaches to keep transaction activities
	/// 	that may differ based on the environments.
	/// </summary>
	public interface IActivityManager
	{
		/// <summary>
		/// 	Gets the current activity.
		/// </summary>
		/// <value>The current activity.</value>
		Activity GetCurrentActivity();
	}
}