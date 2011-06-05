using System.Transactions;
using Castle.Services.Transaction.Tests.Framework;
using Castle.Services.Transaction.Tests.TestClasses;
using Castle.Transactions;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.TransactionManager
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