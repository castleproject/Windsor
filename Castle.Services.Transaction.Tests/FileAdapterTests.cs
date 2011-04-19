using Castle.Services.Transaction.IO;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	[TestFixture]
	public class FileAdapterTests
	{
		[Test]
		public void CtorUseTransactions()
		{
			Assert.That(new FileAdapter().UseTransactions);
		}
	}
}