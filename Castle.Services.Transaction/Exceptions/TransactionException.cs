using System;
using System.Runtime.Serialization;

namespace Castle.Services.Transaction.Exceptions
{
	/// <summary>
	/// Exception thrown when the transaction services code has problems.
	/// </summary>
	[Serializable]
	public class TransactionException : Exception
	{
		private readonly Uri _HelpLink;

		///<summary>
		/// base c'tor
		///</summary>
		public TransactionException()
		{
		}

		/// <summary>
		/// c'tor with message
		/// </summary>
		/// <param name="message"></param>
		public TransactionException(string message) : base(message)
		{
		}

		///<summary>
		/// c'tor with message and a uri (new Uri(...)).
		///</summary>
		///<param name="message"></param>
		///<param name="helpLink">A link relating to the exception/offering guidance.</param>
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