using System;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction
{
	[ContractClass(typeof(ICreatedTransactionContract))]
	public interface ICreatedTransaction
	{
		/// <summary>
		/// Gets the currently active transaction.
		/// </summary>
		ITransaction Transaction { get; }

		/// <summary>
		/// Gets whether the transaction manager from which this instance
		/// was created allows the potential fork-operation.
		/// </summary>
		bool ShouldFork { get; }

		/// <summary>
		/// <para>Call this method from your implementor of the fork-join pattern
		/// for the transaction created, in order to correctly notify the transaction manager of
		/// the activity going on.</para>
		/// <para>
		/// The returned disposable instance needs to be thread-safe.
		/// </para>
		/// </summary>
		/// <returns></returns>
		IDisposable GetForkScope();
	}
}