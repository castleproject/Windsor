using System.Transactions;
using Castle.Transactions.Tests.Framework;
using Castle.Transactions.Tests.TestClasses;
using NUnit.Framework;

namespace Castle.Transactions.Tests.TransactionManager
{
	public class EnlistedResources_Specs : TransactionManager_SpecsBase
	{
		[Test]
		public void IsRolledBack()
		{
			var resource = new ResourceImpl();

			using (var tx = Manager.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}

			Assert.That(resource.RolledBack);
		}
	}
}