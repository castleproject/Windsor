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
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading;

	using Castle.Facilities.Transactions.Tests.Framework;
	using Facilities.Transactions.Tests.TestClasses;
	using NUnit.Framework;

	using Directory = Castle.Facilities.Transactions.IO.Directory;
	using File = Castle.Facilities.Transactions.IO.File;

	[Ignore("Wait for RC")]
	public class FileTransactions_File_Tests : TxFTestFixtureBase
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
			foreach (var filePath in infosCreated)
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

		#endregion

		#region File Tests

		[Test]
		public void CanMove_File()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				Assert.Ignore("TxF not supported");
				return;
			}

			var folder = dllPath.CombineAssert("testing");
			Console.WriteLine(string.Format("Directory \"{0}\"", folder));
			var toFolder = dllPath.CombineAssert("testing2");

			var file = folder.Combine("file");
			Assert.That(File.Exists(file), Is.False);
			var file2 = folder.Combine("file2");
			Assert.That(File.Exists(file2), Is.False);

			File.WriteAllText(file, "hello world");
			File.WriteAllText(file2, "hello world 2");

			infosCreated.Add(file);
			infosCreated.Add(file2);
			infosCreated.Add(toFolder.Combine("file2"));
			infosCreated.Add(toFolder.Combine("file"));
			infosCreated.Add(toFolder);

			using (ITransaction t = new FileTransaction("moving file"))
			{
				Assert.That(File.Exists(toFolder.Combine("file")), Is.False, "Should not exist before move");
				Assert.That(File.Exists(toFolder.Combine("file2")), Is.False, "Should not exist before move");

				(t as IFileAdapter).Move(file, toFolder); // moving file to folder
				(t as IFileAdapter).Move(file2, toFolder.Combine("file2")); // moving file to folder+new file name.

				Assert.That(File.Exists(toFolder.Combine("file")), Is.False, "Should not be visible to the outside");
				Assert.That(File.Exists(toFolder.Combine("file2")), Is.False, "Should not be visible to the outside");

				t.Complete();

				Assert.That(File.Exists(toFolder.Combine("file")), Is.True,
				            "Should be visible to the outside now and since we tried to move it to an existing folder, it should put itself in that folder with its current name.");
				Assert.That(File.Exists(toFolder.Combine("file2")), Is.True, "Should be visible to the outside now.");
			}

			Assert.That(File.ReadAllText(toFolder.Combine("file2")), Is.EqualTo("hello world 2"),
			            "Make sure we moved the contents.");
		}

		[Test]
		public void CreateFileAndReplaceContents()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				Assert.Ignore("TxF not supported");
				return;
			}

			var filePath = testFixturePath.CombineAssert("temp").Combine("temp__");
			infosCreated.Add(filePath);

			// simply write something to to file.
			using (var wr = File.CreateText(filePath))
			{
				wr.WriteLine("Hello");
			}

			using (ITransaction tx = new FileTransaction())
			{
				using (var fs = (tx as IFileAdapter).Create(filePath))
				{
					var str = new UTF8Encoding().GetBytes("Goodbye");
					fs.Write(str, 0, str.Length);
					fs.Flush();
				}

				tx.Complete();
			}

			Assert.That(File.ReadAllLines(filePath)[0], Is.EqualTo("Goodbye"));
		}

		[Test]
		public void CreateFileTransactionally_Rollback()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				Assert.Ignore("TxF not supported");
				return;
			}

			var filePath = testFixturePath.CombineAssert("temp").Combine("temp2");
			infosCreated.Add(filePath);

			// simply write something to to file.
			using (var wr = File.CreateText(filePath))
			{
				wr.WriteLine("Hello");
			}

			using (ITransaction tx = new FileTransaction("Rollback tx"))
			{
				var fa = (IFileAdapter)tx;
				using (var fs = fa.Open(filePath, FileMode.Truncate))
				{
					var str = new UTF8Encoding().GetBytes("Goodbye");
					fs.Write(str, 0, str.Length);
					fs.Flush();
				}

				tx.Rollback();
			}

			Assert.That(File.ReadAllLines(filePath)[0], Is.EqualTo("Hello"));
		}

		[Test]
		public void CreateFileTranscationally_Commit()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				Assert.Ignore("TxF not supported");
				return;
			}

			var filepath = testFixturePath.CombineAssert("temp").Combine("test");

			if (File.Exists(filepath))
				File.Delete(filepath);

			infosCreated.Add(filepath);

			using (ITransaction tx = new FileTransaction("Commit TX"))
			{
				var fa = (IFileAdapter)tx;
				Assert.Fail("TODO");
				//tx.WriteAllText(filepath, "Transactioned file.");
				//tx.Commit();

				//Assert.That(tx.Status == TransactionStatus.Committed);
			}

			Assert.That(File.Exists(filepath), "The file should exists after the transaction.");
			Assert.That(File.ReadAllLines(filepath)[0], Is.EqualTo("Transactioned file."));
		}

		#endregion
	}
}