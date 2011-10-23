// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Transactions.Tests.TransactionManager
{
	using System.IO;
	using System.Transactions;

	using Castle.Transactions.Activities;
	using Castle.Transactions.Tests.TestClasses;

	using NUnit.Framework;

	using TransactionManager = Castle.Transactions.TransactionManager;

	public class transaction_manager_spec
	{
		private ITransactionManager subject;

		[SetUp]
		public void given_manager()
		{
			subject = new TransactionManager(new CallContextActivityManager());
		}

		[TearDown]
		public void tear_down()
		{
			subject.Dispose();
		}

		[Test]
		public void IsRolledBack()
		{
			var resource = new ResourceImpl();

			using (var tx = subject.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}

			Assert.That(resource.RolledBack);
		}

		[Test]
		public void CurrentTransaction_Same_As_First_Transaction()
		{
			using (var t1 = subject.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(subject.CurrentTransaction.Value, Is.SameAs(t1));
			}
		}

		[Test]
		public void CurrentTopTransaction_Same_As_First_Transaction()
		{
			using (var t1 = subject.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				Assert.That(subject.CurrentTopTransaction.Value, Is.SameAs(t1));
			}
		}

		[Test]
		public void TopTransaction_AndTransaction_AreDifferent()
		{
			using (var t1 = subject.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				using (var t2 = subject.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
				{
					Assert.That(subject.CurrentTopTransaction.Value, Is.SameAs(t1));
					Assert.That(subject.CurrentTransaction.Value, Is.SameAs(t2));
					Assert.That(t1, Is.Not.SameAs(t2));
				}
			}
		}

		[Test, Description("This test doesn't explicitly check disposal, but a pass means the bug in previous versions was fixed.")]
		public void Dispose_ITransaction_using_IDisposable_should_run_dispose()
		{
			using (var tx = subject.CreateTransaction().Value.Transaction)
				tx.Complete();

			var newTx = subject.CreateTransaction().Value.Transaction;
		}
	}
}