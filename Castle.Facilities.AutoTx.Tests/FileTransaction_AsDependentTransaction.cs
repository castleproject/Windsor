#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace Castle.Facilities.Transactions.Tests
{
	using System.Transactions;
	using Framework;
	using IO;
	using NUnit.Framework;
	using TestClasses;

	[TestFixture]
	public class FileTransaction_AsDependentTransaction : TxFTestFixtureBase
	{
		private string _DirPath;
		private string _FilePath;

		private ITransactionManager _Tm;

		[SetUp]
		public void Setup()
		{
			_Tm = new Facilities.Transactions.TransactionManager(new TransientActivityManager());

			_DirPath = ".";
			_FilePath = Exts.Combine(_DirPath, "test.txt");
		}

		[Test]
		public void NestedFileTransaction_CanBeCommitted()
		{
			// verify process state
			Assert.That(_Tm.CurrentTransaction.HasValue, Is.False);
			Assert.That(Transaction.Current, Is.Null);

			// actual test code);
			using (var stdTx = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(_Tm.CurrentTransaction.HasValue, Is.True);
				Assert.That(_Tm.CurrentTransaction.Value, Is.EqualTo(stdTx));

				using (var innerTransaction = _Tm.CreateFileTransaction(new DefaultTransactionOptions()).Value.Transaction)
				{
					Assert.That(_Tm.CurrentTransaction.Value, Is.EqualTo(innerTransaction),
					            "Now that we have created a dependent transaction, it's the current tx in the resource manager.");

					// this is supposed to be registered in an IoC container
					var fa = (IFileAdapter)innerTransaction;
					fa.WriteAllText(_FilePath, "Hello world");

					innerTransaction.Complete();
				}
			}

			Assert.That(File.Exists(_FilePath));
			Assert.That(File.ReadAllText(_FilePath), Is.EqualTo("Hello world"));
		}
	}
}