using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Transaction;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class AutoTxFacilityTests
	{
		[Test]
		public void Container_InjectsTransactions_IfTransactionInjectAttribute_is_set()
		{
			WindsorContainer c = new WindsorContainer(new DefaultConfigurationStore());

			c.AddFacility("transactionmanagement", new TransactionFacility());

			c.AddComponent("transactionmanager",
								   typeof(ITransactionManager), typeof(MockTransactionManager));

			c.AddComponent("AClass", typeof(ISomething), typeof(AClass));

			var something = c.Resolve<ISomething>();

			Assert.That(something, Is.Not.Null);

			something.A(null);
			something.B(null);
		}
	}
}