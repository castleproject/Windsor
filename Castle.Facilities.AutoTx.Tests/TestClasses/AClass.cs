using Castle.Services.Transaction;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	[Transactional]
	public class AClass : ISomething
	{
		[Transaction]
		public void A(ITransaction tx)
		{
			Assert.That(tx, Is.Null);
		}

		[Transaction, InjectTransaction]
		public void B(ITransaction tx)
		{
			Assert.That(tx, Is.Not.Null);
		}
	}
}