namespace Castle.Services.Transaction
{
	public sealed class TransactionFailedEventArgs : TransactionEventArgs
	{
		private readonly TransactionException _Exception;

		public TransactionFailedEventArgs(ITransaction tx, TransactionException exception) : base(tx)
		{
			_Exception = exception;
		}

		public TransactionException Exception
		{
			get { return _Exception; }
		}
	}
}