namespace Castle.Transactions.IO.Tests
{
	using System.IO;

	using Castle.IO.Extensions;
	using Castle.Transactions.Activities;

	using NUnit.Framework;

	[TestFixture]
	public class AsDependentTransaction : TxFTestFixtureBase
	{
		private string test_file;
		private ITransactionManager subject;

		[SetUp]
		public void given_manager()
		{
			test_file = ".".Combine("test.txt");
			subject = new TransactionManager(new CallContextActivityManager());
		}

		[Test]
		public void Then_DependentTransaction_CanBeCommitted()
		{
			// verify process state
			Assert.That(subject.CurrentTransaction.HasValue, Is.False);
			Assert.That(System.Transactions.Transaction.Current, Is.Null);

			// actual test code
			using (var stdTx = subject.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(subject.CurrentTransaction.HasValue, Is.True);
				Assert.That(subject.CurrentTransaction.Value, Is.EqualTo(stdTx));

				using (var innerTransaction = subject.CreateFileTransaction().Value)
				{
					Assert.That(subject.CurrentTransaction.Value, Is.EqualTo(innerTransaction),
								"Now that we have created a dependent transaction, it's the current tx in the resource manager.");

					// this is supposed to be registered in an IoC container
					//var fa = (IFileAdapter)innerTransaction;
					//fa.WriteAllText(test_file, "Hello world");

					innerTransaction.Complete();
				}
			}

			Assert.That(File.Exists(test_file));
			Assert.That(File.ReadAllText(test_file), Is.EqualTo("Hello world"));
		}

		[Test]
		public void CompletedState()
		{
			using (IFileTransaction tx = subject.CreateFileTransaction().Value)
			{
				Assert.That(tx.State, Is.EqualTo(TransactionState.Active));
				tx.Complete();
				Assert.That(tx.State, Is.EqualTo(TransactionState.CommittedOrCompleted));
			}
		}

		[TearDown]
		public void tear_down()
		{
			subject.Dispose();
		}
	}
}