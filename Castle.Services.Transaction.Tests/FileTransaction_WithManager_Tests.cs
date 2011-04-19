#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

using System.Transactions;

namespace Castle.Services.Transaction.Tests
{
	using System;
	using System.IO;
	using NUnit.Framework;

	[TestFixture]
	public class FileTransaction_WithManager_Tests
	{
		private string _DirPath;

		// just if I'm curious and want to see that the file exists with my own eyes :p
		private bool _DeleteAtEnd;
		private string _FilePath;

		private ITxManager tm;

		[SetUp]
		public void Setup()
		{
			tm = new TxManager(new TransientActivityManager());

			_DirPath = "../../Transactions/".CombineAssert("tmp");
			_FilePath = _DirPath.Combine("test.txt");
			
			if (File.Exists(_FilePath))
				File.Delete(_FilePath);

			_DeleteAtEnd = true;
		}

		[TearDown]
		public void TearDown()
		{
			if (_DeleteAtEnd && Directory.Exists(_DirPath))
				Directory.Delete(_DirPath, true);
		}

		[Test]
		public void TransactionResources_AreDisposed()
		{
			var resource = new ResourceImpl();
			using (var tx = tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}

			Assert.That(resource.WasDisposed);
			Assert.That(resource.RolledBack);
		}

		[Test]
		public void NestedFileTransaction_CanBeCommitted()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				Assert.Ignore("TxF not supported");
				return;
			}

			// verify process state
			Assert.That(tm.CurrentTransaction.HasValue, Is.False);
			Assert.That(System.Transactions.Transaction.Current, Is.Null);

			// actual test code);
			using (var stdTx = tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(tm.CurrentTransaction.HasValue, Is.True);
				Assert.That(tm.CurrentTransaction.Value, Is.EqualTo(stdTx));

				using (var innerTransaction = tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
				{
					Assert.That(tm.CurrentTransaction.Value, Is.EqualTo(innerTransaction),
								"Now that we have created a dependent transaction, it's the current tx in the resource manager.");

					var fileTransaction = new FileTransaction();
					fileTransaction.WriteAllText(_FilePath, "Hello world");

					innerTransaction.Complete();
					//Assert.That(fileTransaction.Status, Is.EqualTo(TransactionStatus.Committed));
					Assert.That(fileTransaction.IsDisposed);

				}
				// now we can dispose the main transaction.
			}

			Assert.That(File.Exists(_FilePath));
			Assert.That(File.ReadAllLines(_FilePath)[0], Is.EqualTo("Hello world"));
		}
	}
}