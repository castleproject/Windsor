using System;
using System.Threading;
using System.Transactions;
using Castle.Services.Transaction.Internal;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.ExternalSources
{
	[Description("This test fixture passes, rightly so. Durable resource managers that say 'prepared' can't be expected to block on commit, which is why "+
		"there are non-zero enlistment counts in the printout. From http://ayende.com/blog/4528/who-stole-my-transaction")]
	public class WhoStoleMyTransaction_TestCase
	{
		private class EnlistmentTracking : IEnlistmentNotification
		{
			public static int EnlistmentCounts;

			public EnlistmentTracking()
			{
				Interlocked.Increment(ref EnlistmentCounts);
			}

			public void Prepare(PreparingEnlistment preparingEnlistment)
			{
				preparingEnlistment.Prepared();
			}

			public void Commit(Enlistment enlistment)
			{
				Interlocked.Decrement(ref EnlistmentCounts);
				enlistment.Done();
			}

			public void Rollback(Enlistment enlistment)
			{
				Interlocked.Decrement(ref EnlistmentCounts);
				enlistment.Done();
			}

			public void InDoubt(Enlistment enlistment)
			{
				Interlocked.Decrement(ref EnlistmentCounts);
				enlistment.Done();
			}
		}

		private class AnotherEnlistment : IEnlistmentNotification
		{
			private readonly bool _ThrowIt;

			public bool RolledBack;
			public readonly ManualResetEvent HasRolledBack = new ManualResetEvent(false);

			public AnotherEnlistment(bool throwIt)
			{
				_ThrowIt = throwIt;
			}

			public void Prepare(PreparingEnlistment preparingEnlistment)
			{
				if (_ThrowIt) preparingEnlistment.ForceRollback(new ApplicationException("this one went astray"));
				else preparingEnlistment.Prepared();
			}

			public void Commit(Enlistment enlistment)
			{
				if (_ThrowIt)
					Assert.Fail("commit is never called if prepare failed");

				enlistment.Done();
			}

			public void Rollback(Enlistment enlistment)
			{
				RolledBack = true;
				HasRolledBack.Set();
				enlistment.Done();
			}

			public void InDoubt(Enlistment enlistment)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void EnlistmentCounts_TransactionScope()
		{
			var newGuid = Guid.NewGuid();
			for (int i = 0; i < 5; i++)
			{
				using (var tx = new TransactionScope())
				{
					System.Transactions.Transaction.Current.EnlistDurable(newGuid, new EnlistmentTracking(), EnlistmentOptions.None);
					System.Transactions.Transaction.Current.EnlistDurable(newGuid, new EnlistmentTracking(), EnlistmentOptions.None);

					tx.Complete();
				}

				Assert.That(EnlistmentTracking.EnlistmentCounts, Is.EqualTo(0).Or.EqualTo(1).Or.EqualTo(2));
			}
		}

		[Test]
		public void EnlistmentCounts_CommittableTransaction()
		{
			var newGuid = Guid.NewGuid();
			for (int i = 0; i < 5; i++)
			{
				using (var tx = new CommittableTransaction(new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
				using (new TxScope(tx)) // set ambient also!
				{
					System.Transactions.Transaction.Current.EnlistDurable(newGuid, new EnlistmentTracking(), EnlistmentOptions.None);
					System.Transactions.Transaction.Current.EnlistDurable(newGuid, new EnlistmentTracking(), EnlistmentOptions.None);

					tx.Commit();
				}

				Assert.That(EnlistmentTracking.EnlistmentCounts, Is.EqualTo(0).Or.EqualTo(1).Or.EqualTo(2));
			}
		}

		[Test]
		public void IfOneWorks_AndOneFails_FirstShouldRollback()
		{
			var newGuid = Guid.NewGuid();
			for (int i = 0; i < 5; i++)
			{
				using (var tx = new CommittableTransaction(new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
				using (new TxScope(tx)) // set ambient also!
				{
					var first = new AnotherEnlistment(false);
					System.Transactions.Transaction.Current.EnlistDurable(newGuid, first, EnlistmentOptions.None);
					var second = new AnotherEnlistment(true);
					System.Transactions.Transaction.Current.EnlistDurable(newGuid, second, EnlistmentOptions.None);

					try
					{
						tx.Commit();
						Assert.Fail("we have a failing resource");
					}
					catch (TransactionAbortedException e)
					{
						Assert.That(second.RolledBack, Is.False);

						first.HasRolledBack.WaitOne();
						Assert.That(first.RolledBack, Is.True);
					}
				}
			}
			
		}
	}
}