using System.Transactions;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	public class TransactionManager_EnlistedResources
	{
		private ITxManager _Tm;

		[SetUp]
		public void SetUp()
		{
			_Tm = new TxManager(new TransientActivityManager());
		}

		[Test]
		public void TransactionResources_ForFileTransaction_AreDisposed()
		{
			var resource = new ResourceImpl();
			using (var tx = _Tm.CreateFileTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}
			Assert.That(resource.WasDisposed);
			Assert.That(resource.RolledBack);
		}

		[Test]
		public void TransactionResources_AreDisposed()
		{
			var resource = new ResourceImpl();
			using (var tx = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}
			Assert.That(resource.WasDisposed);
			Assert.That(resource.RolledBack);
		}
	}
}