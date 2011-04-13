using System;
using System.Transactions;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class SingleThread_Ambient
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
		}

		[Test]
		public void Automatically_Starts_CommitableTransaction()
		{
			// act & assert
			_Container.Resolve<IMyService>().VerifyInAmbient();
		}

		[Test]
		public void IsDisposed_OnException_And_ActiveDuring_MethodCall()
		{
			var s = _Container.Resolve<IMyService>();
			var txM = _Container.Resolve<ITxManager>();

			System.Transactions.Transaction ambient = null;
			vNextTransaction.ITransaction ourTx = null;

			try
			{
				s.VerifyInAmbient(() =>
				{
				    ambient = System.Transactions.Transaction.Current;
				    ourTx = txM.CurrentTransaction.Value;
					Assert.That(ourTx.State, Is.EqualTo(TransactionState.Active));
				    throw new ApplicationException("should trigger rollback");
				});
			}
			catch (ApplicationException) { }

			Assert.That(ourTx.State, Is.EqualTo(TransactionState.Diposed));
		}

		[Test]
		public void RecursiveTransactions_Inner_Should_Be_DependentTransaction()
		{
			var s = _Container.Resolve<IMyService>();
			var txM = _Container.Resolve<ITxManager>();

			s.VerifyInAmbient(() =>
			{
			    Assert.That(txM.CurrentTransaction.Value.TransactionInformation.Status ==
			                System.Transactions.TransactionStatus.Active);

			    Assert.That(txM.CurrentTransaction.Value.Inner, Is.InstanceOf<CommittableTransaction>());

			    s.VerifyInAmbient(() => Assert.That(txM.CurrentTransaction.Value.Inner, Is.InstanceOf<DependentTransaction>()));
			});
		}
	}
}