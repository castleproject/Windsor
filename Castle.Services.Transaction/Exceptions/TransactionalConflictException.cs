using System;
using System.Runtime.Serialization;

namespace Castle.Services.Transaction
{
	public class TransactionalConflictException : TransactionException
	{
		public TransactionalConflictException(string message) : base(message)
		{
		}

		public TransactionalConflictException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public TransactionalConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}