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

using Castle.Services.Transaction.Tests.Framework;

namespace Castle.Services.Transaction.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using IO;
	using NUnit.Framework;

	[TestFixture, Ignore("Wait for RC")]
	public class FileTransactions_Directory_Tests : TxFTestFixtureBase
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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

			string directoryPath = "testing";
			Assert.That(IO.Directory.Exists(directoryPath), Is.False);

			using (var tx = new FileTransaction())
			{
				(tx as IDirectoryAdapter).Create(directoryPath);
				tx.Dispose();
			}

			Assert.That(!IO.Directory.Exists(directoryPath));
		}

		[Test]
		public void NonExistentDir()
		{
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

			using (var t = new FileTransaction())
			{
				var dir = (t as IDirectoryAdapter);
				Assert.IsFalse(dir.Exists("/hahaha"));
				Assert.IsFalse(dir.Exists("another_non_existent"));
				dir.Create("existing");
				Assert.IsTrue(dir.Exists("existing"));
			}
			// no commit
			Assert.IsFalse(Directory.Exists("existing"));
		}
/*
		[Test, Description("We are not in a distributed transaction if there is no transaction scope.")]
		public void NotUsingTransactionScope_IsNotDistributed_AboveNegated()
		{
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

			// 1. Create directory
			string pr = dllPath.Combine("testing");
			Directory.CreateDirectory(pr);
			Directory.CreateDirectory(pr.Combine("one"));
			Directory.CreateDirectory(pr.Combine("two"));
			Directory.CreateDirectory(pr.Combine("three"));

			// 2. Write contents
			File.WriteAllLines(pr.Combine("one", "fileone"), new[] { "Hello world", "second line" });
			File.WriteAllLines(pr.Combine("one", "filetwo"), new[] { "two", "second line" });
			File.WriteAllLines(pr.Combine("two", "filethree"), new[] { "three", "second line" });

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
            if (Environment.OSVersion.Version.Major < 6)
            {
                Assert.Ignore("TxF not supported");
                return;
            }

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
		} */
	}
}