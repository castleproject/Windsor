using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Castle.Core.IO;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	[TestFixture]
	public class FileTransactions_File_Tests
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

		#endregion

		#region File Tests

		[Test]
		public void CanMove_File()
		{
			string folder = dllPath.CombineAssert("testing");
			Console.WriteLine(string.Format("Directory \"{0}\"", folder));
			string toFolder = dllPath.CombineAssert("testing2");

			string file = folder.Combine("file");
			Assert.That(File.Exists(file), Is.False);
			string file2 = folder.Combine("file2");
			Assert.That(File.Exists(file2), Is.False);

			File.WriteAllText(file, "hello world");
			File.WriteAllText(file2, "hello world 2");

			infosCreated.Add(file);
			infosCreated.Add(file2);
			infosCreated.Add(toFolder.Combine("file2"));
			infosCreated.Add(toFolder.Combine("file"));
			infosCreated.Add(toFolder);

			using (var t = new FileTransaction("moving file"))
			{
				t.Begin();
				Assert.That(File.Exists(toFolder.Combine("file")), Is.False, "Should not exist before move");
				Assert.That(File.Exists(toFolder.Combine("file2")), Is.False, "Should not exist before move");

				(t as IFileAdapter).Move(file, toFolder); // moving file to folder
				(t as IFileAdapter).Move(file2, toFolder.Combine("file2")); // moving file to folder+new file name.

				Assert.That(File.Exists(toFolder.Combine("file")), Is.False, "Should not be visible to the outside");
				Assert.That(File.Exists(toFolder.Combine("file2")), Is.False, "Should not be visible to the outside");

				t.Commit();

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
			string filePath = testFixturePath.CombineAssert("temp").Combine("temp__");
			infosCreated.Add(filePath);

			// simply write something to to file.
			using (StreamWriter wr = File.CreateText(filePath))
				wr.WriteLine("Hello");

			using (var tx = new FileTransaction())
			{
				tx.Begin();

				using (FileStream fs = (tx as IFileAdapter).Create(filePath))
				{
					byte[] str = new UTF8Encoding().GetBytes("Goodbye");
					fs.Write(str, 0, str.Length);
					fs.Flush();
				}

				tx.Commit();
			}

			Assert.That(File.ReadAllLines(filePath)[0], Is.EqualTo("Goodbye"));
		}

		[Test]
		public void CreateFileTransactionally_Rollback()
		{
			string filePath = testFixturePath.CombineAssert("temp").Combine("temp2");
			infosCreated.Add(filePath);

			// simply write something to to file.
			using (StreamWriter wr = File.CreateText(filePath))
				wr.WriteLine("Hello");

			using (var tx = new FileTransaction("Rollback tx"))
			{
				tx.Begin();

				using (FileStream fs = tx.Open(filePath, FileMode.Truncate))
				{
					byte[] str = new UTF8Encoding().GetBytes("Goodbye");
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
			string filepath = testFixturePath.CombineAssert("temp").Combine("test");

			if (File.Exists(filepath))
				File.Delete(filepath);

			infosCreated.Add(filepath);

			using (var tx = new FileTransaction("Commit TX"))
			{
				tx.Begin();
				tx.WriteAllText(filepath, "Transactioned file.");
				tx.Commit();

				Assert.That(tx.Status == TransactionStatus.Committed);
			}

			Assert.That(File.Exists(filepath), "The file should exists after the transaction.");
			Assert.That(File.ReadAllLines(filepath)[0], Is.EqualTo("Transactioned file."));
		}

		#endregion
	}
}