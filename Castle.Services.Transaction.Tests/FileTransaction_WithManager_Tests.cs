#region License
//  Copyright 2004-2010 Castle Project - http:www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http:www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

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

		private DefaultTransactionManager tm;

		[SetUp]
		public void Setup()
		{
			tm = new DefaultTransactionManager(new TransientActivityManager());

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
			var t = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			var resource = new ResourceImpl();
			t.Enlist(resource);
			t.Begin();
			// lalala
			t.Rollback();
			tm.Dispose(t);

			Assert.That(resource.wasDisposed);
		}

		[Test]
		public void NestedFileTransaction_CanBeCommitted()
		{
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

			Assert.That(tm.CurrentTransaction, Is.Null);

			var stdTx = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			stdTx.Begin();

			Assert.That(tm.CurrentTransaction, Is.Not.Null);
			Assert.That(tm.CurrentTransaction, Is.EqualTo(stdTx));

			// invocation.Proceed() in interceptor

			var childTx = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			Assert.That(childTx, Is.InstanceOf(typeof(ChildTransaction)));
			Assert.That(tm.CurrentTransaction, Is.EqualTo(childTx), 
			            "Now that we have created a child, it's the current tx.");

			var txF = new FileTransaction();
			childTx.Enlist(new FileResourceAdapter(txF));
			childTx.Begin();

			txF.WriteAllText(_FilePath, "Hello world");

			childTx.Commit();
			stdTx.Commit();

			Assert.That(File.Exists(_FilePath));
			Assert.That(File.ReadAllLines(_FilePath)[0], Is.EqualTo("Hello world"));

			// first we need to dispose the child transaction.
			tm.Dispose(childTx);

			// now we can dispose the main transaction.
			tm.Dispose(stdTx);

			Assert.That(txF.Status, Is.EqualTo(TransactionStatus.Committed));
			Assert.That(txF.IsDisposed);
		}

		[Test]
		public void UsingNestedTransaction_FileTransactionOnlyVotesToCommit()
		{
			// TODO Implement proper exception handling when file transaction is voted to commit
		}

		[Test]
		public void BugWhenResourceFailsAndTransactionCommits()
		{
			var tx = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);

		}
	}
}