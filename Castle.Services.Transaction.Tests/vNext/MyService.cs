using System;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class MyService : IMyService
	{
		[Transaction]
		public void VerifyInAmbient()
		{
			Assert.That(System.Transactions.Transaction.Current != null,
				"The current transaction mustn't be null.");
		}

		[Transaction]
		public void VerifyInAmbient(Action a)
		{
			Assert.That(System.Transactions.Transaction.Current != null,
				"The current transaction mustn't be null.");

			a();
		}
	}
}