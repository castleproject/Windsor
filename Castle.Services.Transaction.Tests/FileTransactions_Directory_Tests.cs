using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Castle.Services.Transaction.IO;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	[TestFixture]
	public class FileTransactions_Directory_Tests
	{
		#region Setup/Teardown

		private string dllPath;
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
			dllPath.Combine("..\\..\\Kernel");
		}

		#endregion

		[Test]
		public void NoCommit_MeansNoDirectory()
		{
			string directoryPath = "testing";
			Assert.That(Directory.Exists(directoryPath), Is.False);

			using (var tx = new FileTransaction())
			{
				tx.Begin();
				(tx as IDirectoryAdapter).Create(directoryPath);
			}

			Assert.That(!Directory.Exists(directoryPath));
		}

		[Test]
		public void NonExistentDir()
		{
			using (var t = new FileTransaction())
			{
				t.Begin();
				var dir = (t as IDirectoryAdapter);
				Assert.IsFalse(dir.Exists("/hahaha"));
				Assert.IsFalse(dir.Exists("another_non_existent"));
				dir.Create("existing");
				Assert.IsTrue(dir.Exists("existing"));
			}
			// no commit
			Assert.IsFalse(Directory.Exists("existing"));
		}

		[Test, Description("We are not in a distributed transaction if there is no transaction scope.")]
		public void NotUsingTransactionScope_IsNotDistributed_AboveNegated()
		{
			using (var tx = new FileTransaction("Not distributed transaction"))
			{
				tx.Begin();
				Assert.That(tx.IsAmbient, Is.False);
				tx.Commit();
			}
		}

		[Test]
		public void ExistingDirWithTrailingBackslash()
		{
			// From http://msdn.microsoft.com/en-us/library/aa364419(VS.85).aspx
			// An attempt to open a search with a trailing backslash always fails.
			// --> So I need to make it succeed.
			using (var t = new FileTransaction())
			{
				t.Begin();
				var dir = t as IDirectoryAdapter;
				dir.Create("something");
				Assert.That(dir.Exists("something"));
				Assert.That(dir.Exists("something\\"));
			}
		}

		[Test]
		public void CreatingFolder_InTransaction_AndCommitting_MeansExistsAfter()
		{
			string directoryPath = "testing";
			Assert.That(Directory.Exists(directoryPath), Is.False);

			using (var tx = new FileTransaction())
			{
				tx.Begin();
				(tx as IDirectoryAdapter).Create(directoryPath);
				tx.Commit();
			}

			Assert.That(Directory.Exists(directoryPath));

			Directory.Delete(directoryPath);
		}

		[Test]
		public void CanCreate_AndFind_Directory_WithinTx()
		{
			using (var tx = new FileTransaction("s"))
			{
				tx.Begin();
				Assert.That((tx as IDirectoryAdapter).Exists("something"), Is.False);
				(tx as IDirectoryAdapter).Create("something");
				Assert.That((tx as IDirectoryAdapter).Exists("something"));
				tx.Rollback();
			}
		}

		[Test]
		public void CanCreateDirectory_NLengths_DownInNonExistentDirectory()
		{
			string directoryPath = "testing/apa/apa2";
			Assert.That(Directory.Exists(directoryPath), Is.False);

			using (var t = new FileTransaction())
			{
				t.Begin();
				(t as IDirectoryAdapter).Create(directoryPath);
				t.Commit();
			}

			Assert.That(Directory.Exists(directoryPath));
			Directory.Delete(directoryPath);
		}
		[Test]
		public void CanDelete_NonRecursively_EmptyDir()
		{
			// 1. create dir
			string dir = dllPath.CombineAssert("testing");

			// 2. test it
			using (var t = new FileTransaction("Can delete empty directory"))
			{
				IDirectoryAdapter da = t;
				t.Begin();
				Assert.That(da.Delete(dir, false), "Successfully deleted.");
				t.Commit();
			}
		}

		[Test]
		public void CanDelete_Recursively()
		{
			// 1. Create directory
			string pr = dllPath.Combine("testing");
			Directory.CreateDirectory(pr);
			Directory.CreateDirectory(pr.Combine("one"));
			Directory.CreateDirectory(pr.Combine("two"));
			Directory.CreateDirectory(pr.Combine("three"));

			// 2. Write contents
			File.WriteAllLines(Exts.Combine(pr, "one").Combine("fileone"), new[] { "Hello world", "second line" });
			File.WriteAllLines(Exts.Combine(pr, "one").Combine("filetwo"), new[] { "two", "second line" });
			File.WriteAllLines(Exts.Combine(pr, "two").Combine("filethree"), new[] { "three", "second line" });

			// 3. test
			using (var t = new FileTransaction())
			{
				t.Begin();
				Assert.IsTrue((t as IDirectoryAdapter).Delete(pr, true));
				t.Commit();
			}
		}

		[Test]
		public void CanNotDelete_NonRecursively_NonEmptyDir()
		{
			// 1. create dir and file
			string dir = dllPath.CombineAssert("testing");
			string file = dir.Combine("file");
			File.WriteAllText(file, "hello");

			// 2. test it
			using (var t = new FileTransaction("Can not delete non-empty directory"))
			{
				IDirectoryAdapter da = t;
				t.Begin();
				Assert.That(da.Delete(dir, false),
							Is.False,
							"Did not delete non-empty dir.");
				IFileAdapter fa = t;
				fa.Delete(file);

				Assert.That(da.Delete(dir, false),
							"After deleting the file in the folder, the folder is also deleted.");

				t.Commit();
			}
		}
	}
}