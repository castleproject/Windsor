using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Transactions;
using Castle.Core.IO;
using NUnit.Framework;
using Path=Castle.Core.IO.Path;

namespace Castle.Services.Transaction.Tests
{
	[TestFixture]
	public class FileTransactionTests
	{
		#region Setup/Teardown

		private string dllPath;
		private string testFixturePath;
		private readonly List<string> infosCreated = new List<string>();
		private static volatile object serializer = new object();

		[SetUp]
		public void CleanOutListEtc()
		{
			Monitor.Enter(serializer);
			infosCreated.Clear();
		}

		[TearDown]
		public void RemoveAllCreatedFiles()
		{
			foreach (string filePath in infosCreated)
			{
				if (File.Exists(filePath))
					File.Delete(filePath);
				else if (Directory.Exists(filePath))
					Directory.Delete(filePath);
			}

			if (Directory.Exists("testing"))
				Directory.Delete("testing", true);

			Monitor.Exit(serializer);
		}

		[TestFixtureSetUp]
		public void Setup()
		{
			dllPath = Environment.CurrentDirectory;
			testFixturePath = dllPath.Combine("..\\..\\Kernel");
		}

		private class R : IResource
		{
			public void Start() { }
			public void Commit() { }
			public void Rollback() { throw new Exception("Expected."); }
		}

		#endregion

		#region State and Rollbacks

		[Test]
		public void CTorTests()
		{
			var t = new FileTransaction();
			Assert.That(t.Status, Is.EqualTo(TransactionStatus.NoTransaction));
		}

		[Test]
		public void CannotCommitAfterSettingRollbackOnly()
		{
			using (var tx = new FileTransaction())
			{
				tx.Begin();
				tx.SetRollbackOnly();
				Assert.Throws(typeof(TransactionException), tx.Commit,
							  "Should not be able to commit after rollback is set.");
			}
		}

		[Test]
		public void FailingResource_TxStillRolledBack()
		{
			using (var tx = new FileTransaction())
			{
				tx.Enlist(new R());
				tx.Begin();
				try
				{
					try
					{
						tx.Rollback();
						Assert.Fail("Tests is wrong or the transaction doesn't rollback resources.");
					}
					catch (Exception)
					{
					}

					Assert.That(tx.Status == TransactionStatus.RolledBack);
				}
				catch (RollbackResourceException rex)
				{
					// good.
					Assert.That(rex.FailedResource[0].First, Is.InstanceOf(typeof (R)));
				}
			}
		}

		[Test]
		public void FileModeOpenOrCreateEqualsOpenAlways()
		{
			Assert.That((int) FileMode.OpenOrCreate, Is.EqualTo(4));
		}

		[Test]
		public void InvalidStateOnCreate_Throws()
		{
			using (var tx = new FileTransaction())
			{
				Assert.Throws(typeof (TransactionException), () => (tx as IDirectoryAdapter).Create("lol"),
				              "The transaction hasn't begun, throws.");
			}
		}

		#endregion

		#region Ambient Transactions

		[Test]
		public void Using_TransactionScope_IsDistributed_AlsoTestingStatusWhenRolledBack()
		{
			using (new TransactionScope())
			{
				using (var tx = new FileTransaction())
				{
					tx.Begin();

					Assert.That(tx.IsAmbient);

					tx.Rollback();
					Assert.That(tx.IsRollbackOnlySet);
					Assert.That(tx.Status, Is.EqualTo(TransactionStatus.RolledBack));
				}
			}
		}

		[Test]
		public void Using_NormalStates()
		{
			using (var tx = new FileTransaction())
			{
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.NoTransaction));
				tx.Begin();
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.Active));
				tx.Commit();
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.Committed));
			}
		}

		#endregion

		#region Region Ignored

		[Test, Ignore("Not completely implemented.")]
		public void CanMoveDirectory()
		{
			string dir1 = dllPath.CombineAssert("a");
			string dir2 = dllPath.Combine("b");

			Assert.That(Directory.Exists(dir2), Is.False);
			Assert.That(File.Exists(dir2), Is.False, "Lingering files should not be allowed to disrupt the testing.");


			string aFile = dir1.Combine("file");
			File.WriteAllText(aFile, "I should also be moved.");
			infosCreated.Add(aFile);

			using (var t = new FileTransaction("moving tx"))
			{
				t.Begin();

				(t as IDirectoryAdapter).Move(dir1, dir2);
				Assert.IsFalse(Directory.Exists(dir2), "The directory should not yet exist.");

				t.Commit();
				Assert.That(Directory.Exists(dir2), "Now after committing it should.");
				infosCreated.Add(dir2);

				Assert.That(File.Exists(dir2.Combine(Path.GetFileName(aFile))), "And so should the file in the directory.");
			}
		}

		// http://msdn.microsoft.com/en-us/library/aa365536%28VS.85%29.aspx
		[Test, Ignore("MSDN is wrong in saying: \"If a non-transacted thread modifies the file before the transacted thread does, "
					  + "and the file is still open when the transaction attempts to open it, "
					  + "the transaction receives the error ERROR_TRANSACTIONAL_CONFLICT.\"... "
					  + "This test proves the error in this statement. Actually, from testing the rest of the code, it's clear that "
					  + "the error comes for the opposite; when a transacted thread modifies before a non-transacted thread.")]
		public void TwoTransactions_SameName_FirstSleeps()
		{
			var t1_started = new ManualResetEvent(false);
			var t2_started = new ManualResetEvent(false);
			var t2_done = new ManualResetEvent(false);
			Exception e = null;

			// non transacted thread
			var t1 = new Thread(() =>
			{
				try
				{
					// modifies the file
					using (var fs = File.OpenWrite("abb"))
					{
						Console.WriteLine("t2 start");
						Console.Out.Flush();
						t2_started.Set(); // before the transacted thread does
						Console.WriteLine("t2 wait for t1 to start"); Console.Out.Flush();
						t1_started.WaitOne();
						fs.Write(new byte[] { 0x1 }, 0, 1);
						fs.Close();
					}
				}
				catch (Exception ee)
				{
					e = ee;
				}
				finally
				{
					Console.WriteLine("t2 finally"); Console.Out.Flush();
					t2_started.Set();
				}
			});

			t1.Start();

			using (var t = new FileTransaction())
			{
				t.Begin();

				Console.WriteLine("t1 wait for t2 to start"); Console.Out.Flush();
				t2_started.WaitOne();

				try
				{
					Console.WriteLine("t1 started");
					// the transacted thread should receive ERROR_TRANSACTIONAL_CONFLICT, but it gets permission denied.
					using (var fs = (t as IFileAdapter).Create("abb"))
					{
						fs.WriteByte(0x2);
					}
				}
				finally
				{
					Console.WriteLine("t1 finally"); Console.Out.Flush();
					t1_started.Set();
				}

				t.Commit();
			}


			if (e != null)
			{
				Console.WriteLine(e);
				Assert.Fail(e.Message);
			}
		}

		#endregion
	}
}