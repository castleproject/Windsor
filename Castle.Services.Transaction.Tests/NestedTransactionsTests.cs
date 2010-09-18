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
namespace Castle.Services.Transaction.Tests
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class NestedTransactionsTests
	{
		private DefaultTransactionManager tm;

		[SetUp]
		public void Init()
		{
			tm = new DefaultTransactionManager(new TransientActivityManager());
		}

		[Test]
		public void NestedRequiresWithCommits()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue( root is TransactionBase );
			root.Begin();

			ITransaction child1 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue( child1 is ChildTransaction );
			Assert.IsTrue( child1.IsChildTransaction );
			child1.Begin();
			
			ITransaction child2 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue( child2 is ChildTransaction );
			Assert.IsTrue( child2.IsChildTransaction );
			child2.Begin();

			child2.Commit();
			child1.Commit();
			root.Commit();
		}

		[Test]
		public void NestedRequiresAndRequiresNew()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue(root is TransactionBase);
			root.Begin();

			ITransaction child1 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue( child1 is ChildTransaction );
			child1.Begin();
			
			ITransaction innerRoot = tm.CreateTransaction( TransactionMode.RequiresNew, IsolationMode.Unspecified );
			Assert.IsFalse( innerRoot is ChildTransaction );
			innerRoot.Begin();

			ITransaction child2 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue( child2 is ChildTransaction );
			child2.Begin();

			child2.Commit();
			innerRoot.Commit();

			child1.Commit();
			root.Commit();
		}

		[Test]
		public void SameResourcesShared_BetweenParentAndChild_ParentsResponsibility()
		{
			var resource = new ResourceImpl();

			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			root.Begin();
			root.Enlist(resource);

			var child1 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			Assert.IsTrue( child1 is ChildTransaction );
			child1.Enlist(resource);
			child1.Begin();
			
			child1.Commit();
			root.Commit();
		}

		[Test]
		public void NotSupportedAndNoActiveTransaction()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.NotSupported, IsolationMode.Unspecified );
			Assert.IsNull( root );
		}

		[Test]
		[ExpectedException( typeof(TransactionModeUnsupportedException) )]
		public void NotSupportedAndActiveTransaction()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			root.Begin();

			tm.CreateTransaction( TransactionMode.NotSupported, IsolationMode.Unspecified );
		}

		[Test]
		[ExpectedException( typeof(TransactionException) )]
		public void NestedRollback_RollingAChildBack_TryingToCommitRoot_Fails()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			root.Begin();

			ITransaction child1 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			child1.Begin();
			
			ITransaction child2 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			child2.Begin();

			child2.Rollback();
			child1.Commit();
			root.Commit(); // Can't perform
		}

		[Test]
		[ExpectedException( typeof(ArgumentException) )]
		public void InvalidDispose1()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			root.Begin();

			ITransaction child1 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			child1.Begin();
			
			ITransaction child2 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			child2.Begin();

			tm.Dispose(child1);
		}

		[Test]
		[ExpectedException( typeof(ArgumentException) )]
		public void InvalidDispose2()
		{
			ITransaction root = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			root.Begin();

			ITransaction child1 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			child1.Begin();
			
			ITransaction child2 = tm.CreateTransaction( TransactionMode.Requires, IsolationMode.Unspecified );
			child2.Begin();

			tm.Dispose(root);
		}

		[Test]
		public void WhenOneResourceFails_OtherResourcesAreNotCommitted()
		{
			var first = new ResourceImpl();
			var rfail = new ThrowsExceptionResourceImpl(true, false);
			var rsucc = new ResourceImpl();

			var t = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			t.Enlist(first);
			t.Enlist(rfail);
			t.Enlist(rsucc);

			t.Begin();

			Assert.That(rfail.Started);
			Assert.That(rsucc.Started);

			Assert.Throws(typeof(CommitResourceException), t.Commit);

			Assert.That(first.Committed);
			Assert.That(rfail.Committed, Is.False);
			Assert.That(rsucc.Committed, Is.False);
		}

		[Test]
		public void SynchronizationsAndCommit_NestedTransaction()
		{
			ITransaction root =
				tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			Assert.IsTrue(root is TalkativeTransaction);
			root.Begin();

			ITransaction child1 = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			Assert.IsTrue(child1 is ChildTransaction);
			Assert.IsTrue(child1.IsChildTransaction);
			child1.Begin();

			SynchronizationImpl sync = new SynchronizationImpl();

			child1.RegisterSynchronization(sync);

			Assert.AreEqual(DateTime.MinValue, sync.Before);
			Assert.AreEqual(DateTime.MinValue, sync.After);

			child1.Commit();
			root.Commit();

			Assert.IsTrue(sync.Before > DateTime.MinValue);
			Assert.IsTrue(sync.After > DateTime.MinValue);
		}

		[Test]
		public void SynchronizationsAndRollback_NestedTransaction()
		{
			ITransaction root =
				tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			Assert.IsTrue(root is TalkativeTransaction);
			root.Begin();

			ITransaction child1 = tm.CreateTransaction(TransactionMode.Requires, IsolationMode.Unspecified);
			Assert.IsTrue(child1 is ChildTransaction);
			Assert.IsTrue(child1.IsChildTransaction);
			child1.Begin();

			SynchronizationImpl sync = new SynchronizationImpl();

			child1.RegisterSynchronization(sync);

			Assert.AreEqual(DateTime.MinValue, sync.Before);
			Assert.AreEqual(DateTime.MinValue, sync.After);

			child1.Rollback();
			root.Rollback();

			Assert.IsTrue(sync.Before > DateTime.MinValue);
			Assert.IsTrue(sync.After > DateTime.MinValue);
		}
	}
}
