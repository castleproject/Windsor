using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	internal class TransactionManager_TransactionDependentAndStackState
	{
		private ITxManager _Tm;

		[SetUp]
		public void SetUp()
		{
			_Tm = new TxManager(new TransientActivityManager());
		}

		[Test]
		public void TearDown()
		{
			_Tm.Dispose();
		}

		[Test]
		public void CurrentTransaction_Same_As_First_Transaction()
		{
			using (var t1 = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
				Assert.That(_Tm.CurrentTransaction.Value, Is.SameAs(t1));
		}

		[Test]
		public void CurrentTopTransaction_Same_As_First_Transaction()
		{
			using (var t1 = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
				Assert.That(_Tm.CurrentTopTransaction.Value, Is.SameAs(t1));
		}

		[Test]
		public void TopTransaction_AndTransaction_AreDifferent()
		{
			using (var t1 = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			using (var t2 = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(_Tm.CurrentTopTransaction.Value, Is.SameAs(t1));
				Assert.That(_Tm.CurrentTransaction.Value, Is.SameAs(t2));
				Assert.That(t1, Is.Not.SameAs(t2));
			}
		}
	}
}