using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using NUnit.Framework;

namespace Castle.Facilities.Transactions.Tests.TestClasses
{
	public class ConcreteService
	{
		private readonly ITransactionManager _Manager;

		public ConcreteService(ITransactionManager manager)
		{
			Contract.Ensures(_Manager != null);
			_Manager = manager;
		}

		[Transaction]
		public virtual ITransaction VerifyInAmbient()
		{
			Assert.That(System.Transactions.Transaction.Current != null,
			            "The current transaction mustn't be null.");

			Assert.That(_Manager.CurrentTransaction != null,
			            "The current transaction in the transaction manager mustn't be null.");

			return _Manager.CurrentTransaction.Value;
		}

		[Transaction]
		public virtual void VerifyInAmbient(Action a)
		{
			Assert.That(System.Transactions.Transaction.Current != null,
			            "The current transaction mustn't be null.");

			a();
		}
	}
}