using Castle.Services.Transaction.IO;
using Castle.Services.Transaction.Tests.Framework;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	[TestFixture]
	public class FileTransaction_AsDependentTransaction : TxFTestFixtureBase
	{
		private string _DirPath;
		private string _FilePath;

		private ITransactionManager _Tm;

		[SetUp]
		public void Setup()
		{
			_Tm = new TransactionManager(new TransientActivityManager());

			_DirPath = ".";
			_FilePath = _DirPath.Combine("test.txt");
		}

		[Test]
		public void NestedFileTransaction_CanBeCommitted()
		{
			// verify process state
			Assert.That(_Tm.CurrentTransaction.HasValue, Is.False);
			Assert.That(System.Transactions.Transaction.Current, Is.Null);

			// actual test code
			using (var stdTx = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(_Tm.CurrentTransaction.HasValue, Is.True);
				Assert.That(_Tm.CurrentTransaction.Value, Is.EqualTo(stdTx));

				using (var innerTransaction = _Tm.CreateFileTransaction(new DefaultTransactionOptions()).Value.Transaction)
				{
					Assert.That(_Tm.CurrentTransaction.Value, Is.EqualTo(innerTransaction),
					            "Now that we have created a dependent transaction, it's the current tx in the resource manager.");

					// this is supposed to be registered in an IoC container
					var fa = (IFileAdapter) innerTransaction;
					fa.WriteAllText(_FilePath, "Hello world");

					innerTransaction.Complete();
				}
			}

			Assert.That(File.Exists(_FilePath));
			Assert.That(File.ReadAllText(_FilePath), Is.EqualTo("Hello world"));
		}
	}
}