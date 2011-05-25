using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Castle.Facilities.Transactions.Contracts;

namespace Castle.Facilities.Transactions.Internal
{
	/// <summary>
	/// An interface specifying whether the <see cref="ITransaction"/> implementation
	/// knows about its dependents. If the transaction class does not implement this interface
	/// then dependent transcations that fail will not be awaited on the main thread, but instead
	/// on the finalizer thread (not good!).
	/// </summary>
	[ContractClass(typeof(IDependentAwareContract))]
	public interface IDependentAware
	{
		/// <summary>
		/// Registers a dependent task to wait for after Complete or Rollback has been called.
		/// </summary>
		/// <param name="task">The task to await.</param>
		void RegisterDependent(Task task);
	}
}