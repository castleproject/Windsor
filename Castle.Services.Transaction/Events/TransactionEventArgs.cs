using System;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// Event args for a transaction event.
	/// </summary>
	public class TransactionEventArgs : EventArgs
	{
		private readonly ITransaction _Tx;

		public TransactionEventArgs(ITransaction tx)
		{
			_Tx = tx;
		}

		public ITransaction Transaction
		{
			get { return _Tx; }
		}
	}
}