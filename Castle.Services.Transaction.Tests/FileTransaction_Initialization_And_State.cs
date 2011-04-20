using System.Transactions;
using Castle.Services.Transaction.Tests.Framework;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	public class FileTransaction_Initialization_And_State : TxFTestFixtureBase
	{

		[Test]
		public void CompletedState()
		{
			using (ITransaction tx = new FileTransaction())
			{
				Assert.That(tx.State, Is.EqualTo(TransactionState.Active));
				tx.Complete();
				Assert.That(tx.State, Is.EqualTo(TransactionState.CommittedOrCompleted));
			}
		}
	}
}