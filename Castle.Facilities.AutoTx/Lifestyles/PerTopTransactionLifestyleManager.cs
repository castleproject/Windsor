using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Lifestyles
{
	/// <summary>
	/// A lifestyle manager for every top transaction in the current call context. This lifestyle is great
	/// for components that are thread-safe and need to monitor/handle items in both the current thread
	/// and any forked method invocations. It's also favoring memory if your application is single threaded,
	/// as there's no need to create a new component every sub-transaction. (this refers to the Fork=true option
	/// on the TransactionAttribute).
	/// </summary>
	public class PerTopTransactionLifestyleManager : PerTransactionLifestyleManagerBase
	{
		public PerTopTransactionLifestyleManager(ITxManager manager)
			: base(manager)
		{
			Contract.Requires(manager != null);
		}

		#region Overrides of PerTransactionLifestyleManagerBase

		protected internal override Maybe<ITransaction> GetSemanticTransactionForLifetime()
		{
			if (_Disposed)
				throw new ObjectDisposedException("PerTopTransactionLifestyleManager", "The lifestyle manager is disposed and cannot be used.");

			return _Manager.CurrentTopTransaction;
		}

		#endregion
	}
}