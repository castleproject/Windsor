using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Lifestyles
{
	/// <summary>
	/// A lifestyle manager that resolves a fresh instance for every transaction. In my opinion, this 
	/// is the most semantically correct option of the two per-transaction lifestyle managers: it's possible
	/// to audit your code to verify that sub-sequent calls to services don't start new transactions on their own.
	/// With this lifestyle, code executing in other threads work as expected, as no instances are shared accross these
	/// threads (this refers to the Fork=true option on the TransactionAttribute).
	/// </summary>
	public class PerTransactionLifestyleManager : PerTransactionLifestyleManagerBase
	{
		public PerTransactionLifestyleManager(ITxManager manager) 
			: base(manager)
		{
			Contract.Requires(manager != null);
		}

		#region Overrides of PerTransactionLifestyleManagerBase

		protected internal override Maybe<ITransaction> GetSemanticTransactionForLifetime()
		{
			if (_Disposed)
				throw new ObjectDisposedException("PerTransactionLifestyleManager", "The lifestyle manager is disposed and cannot be used.");

			return _Manager.CurrentTransaction;
		}

		#endregion
	}
}