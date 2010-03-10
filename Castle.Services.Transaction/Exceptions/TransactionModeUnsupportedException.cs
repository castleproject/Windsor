using System;
using System.Runtime.Serialization;

namespace Castle.Services.Transaction
{
	public class TransactionModeUnsupportedException : TransactionException
	{
		public TransactionModeUnsupportedException(string message) : base(message)
		{
		}

		public TransactionModeUnsupportedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public TransactionModeUnsupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}