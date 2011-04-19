using System;

namespace Castle.Services.Transaction
{
	public class TxScope : IDisposable
	{
		private System.Transactions.Transaction prev;

		public TxScope(System.Transactions.Transaction curr)
		{
			prev = System.Transactions.Transaction.Current;
			System.Transactions.Transaction.Current = curr;
		}

		public void Dispose()
		{
			System.Transactions.Transaction.Current = prev;
		}
	}
}