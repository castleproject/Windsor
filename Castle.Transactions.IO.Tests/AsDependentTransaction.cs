namespace Castle.Transactions.IO.Tests
{
	using NUnit.Framework;

	[TestFixture]
	public class AsDependentTransaction : TxFTestFixtureBase
	{
		private string _ATestFile;

		[SetUp]
		public void Given()
		{
			_ATestFile = ".".Combine("test.txt");
		}

		[Test]
		public void Then_DependentTransaction_CanBeCommitted()
		{
			// verify process state
			Assert.That(Manager.CurrentTransaction.HasValue, Is.False);
			Assert.That(System.Transactions.Transaction.Current, Is.Null);

			// actual test code
			using (var stdTx = Manager.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(Manager.CurrentTransaction.HasValue, Is.True);
				Assert.That(Manager.CurrentTransaction.Value, Is.EqualTo(stdTx));

				using (var innerTransaction = Manager.CreateFileTransaction(new DefaultTransactionOptions()).Value.Transaction)
				{
					Assert.That(Manager.CurrentTransaction.Value, Is.EqualTo(innerTransaction),
					            "Now that we have created a dependent transaction, it's the current tx in the resource manager.");

					// this is supposed to be registered in an IoC container
					var fa = (IFileAdapter) innerTransaction;
					fa.WriteAllText(_ATestFile, "Hello world");

					innerTransaction.Complete();
				}
			}

			Assert.That(File.Exists(_ATestFile));
			Assert.That(File.ReadAllText(_ATestFile), Is.EqualTo("Hello world"));
		}
	}
}