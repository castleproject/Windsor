using System;
using System.Runtime.Serialization;

namespace Castle.Services.vNextTransaction
{
	[Serializable]
	public class TransactionException : Exception
	{
		private readonly Uri _HelpLink;

		public TransactionException(string message, Uri helpLink) : base(message)
		{
			_HelpLink = helpLink;
		}

		public TransactionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public TransactionException(string message, Exception innerException, Uri helpLink) : base(message, innerException)
		{
			_HelpLink = helpLink;
		}

		protected TransactionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_HelpLink = (Uri)info.GetValue("HelpLink", typeof (Uri));
		}

		public new Uri HelpLink
		{
			get { return _HelpLink; }
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("HelpLink", HelpLink);
			base.GetObjectData(info, context);
		}
	}
}