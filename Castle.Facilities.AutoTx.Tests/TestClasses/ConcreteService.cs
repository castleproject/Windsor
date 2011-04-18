using System;
using System.Diagnostics.Contracts;
using Castle.Services.vNextTransaction;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class ConcreteService
	{
		private readonly ITxManager _Manager;

		public ConcreteService(ITxManager manager)
		{
			Contract.Ensures(_Manager != null);
			_Manager = manager;
		}

		[vNextTransaction.Transaction]
		public virtual vNextTransaction.ITransaction VerifyInAmbient()
		{
			Assert.That(System.Transactions.Transaction.Current != null,
			            "The current transaction mustn't be null.");

			Assert.That(_Manager.CurrentTransaction != null,
			            "The current transaction in the transaction manager mustn't be null.");

			return _Manager.CurrentTransaction.Value;
		}

		[vNextTransaction.Transaction]
		public virtual void VerifyInAmbient(Action a)
		{
			Assert.That(System.Transactions.Transaction.Current != null,
			            "The current transaction mustn't be null.");

			a();
		}
	}
}