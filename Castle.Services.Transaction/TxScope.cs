using System;
using log4net;

namespace Castle.Services.Transaction
{
	public sealed class TxScope : IDisposable
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TxScope));

		private System.Transactions.Transaction prev;

		public TxScope(System.Transactions.Transaction curr)
		{
			prev = System.Transactions.Transaction.Current;
			System.Transactions.Transaction.Current = curr;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isManaged)
		{
			if (!isManaged)
			{
				_Logger.Warn("TxScope Dispose wasn't called from managed context! You need to make sure that you dispose the scope, " 
					+ "or you will break the Transaction.Current invariant of the framework and your own code by extension.");

				return;
			}
			System.Transactions.Transaction.Current = prev;
		}
	}
}