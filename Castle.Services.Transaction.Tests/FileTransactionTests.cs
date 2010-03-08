using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

		#endregion

		private string dllPath;
		private string testFixturePath;
		private readonly List<string> infosCreated = new List<string>();
		private volatile object serializer = new object();

		[TestFixtureSetUp]
		public void Setup()
		{
			dllPath = Environment.CurrentDirectory;
			testFixturePath = dllPath.Combine("..\\..\\Kernel");
		}

		private class R : IResource
		{
			#region IResource Members

			public void Start()
			{
			}

			public void Commit()
			{
			}

			public void Rollback()
			{
				throw new Exception("Expected.");
			}

			#endregion
		}

		#region Directory Operations

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

		#endregion

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
			File.WriteAllLines(Exs.Combine(pr, "one").Combine("fileone"), new[] {"Hello world", "second line"});
			File.WriteAllLines(Exs.Combine(pr, "one").Combine("filetwo"), new[] {"two", "second line"});
			File.WriteAllLines(Exs.Combine(pr, "two").Combine("filethree"), new[] {"three", "second line"});

			// 3. test
			using (var t = new FileTransaction())
			{
				t.Begin();
				Assert.IsTrue((t as IDirectoryAdapter).Delete(pr, true));
				t.Commit();
			}
		}

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

		[Test]
		public void CannotCommitAfterSettingRollbackOnly()
		{
			using (var tx = new FileTransaction())
			{
				tx.Begin();
				tx.SetRollbackOnly();
				Assert.Throws(typeof (TransactionException), tx.Commit,
				              "Should not be able to commit after rollback is set.");
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
		public void CTorTests()
		{
			var t = new FileTransaction();
			Assert.That(t.Status, Is.EqualTo(TransactionStatus.NoTransaction));
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
					Assert.That(rex.FailedResources[0], Is.InstanceOf(typeof (R)));
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

		// Directories

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
				Assert.That(tx.DistributedTransaction, Is.False);
				tx.Commit();
			}
		}

		// http://msdn.microsoft.com/en-us/library/aa365536%28VS.85%29.aspx
		[Test, Ignore("MSDN is wrong in saying: \"If a non-transacted thread modifies the file before the transacted thread does, " 
			+ "and the file is still open when the transaction attempts to open it, " 
			+ "the transaction receives the error ERROR_TRANSACTIONAL_CONFLICT.\"... "
			+"This test proves the error in this statement.")]
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
						fs.Write(new byte[] {0x1}, 0, 1);
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

		[Test]
		public void Using_TransactionScope_IsDistributed_AlsoTestingStatusWhenRolledBack()
		{
			using (new TransactionScope())
			{
				using (var tx = new FileTransaction())
				{
					tx.Begin();

					Assert.That(tx.DistributedTransaction);

					tx.Rollback();
					Assert.That(tx.IsRollbackOnlySet);
					Assert.That(tx.Status, Is.EqualTo(TransactionStatus.RolledBack));
				}
			}
		}

		[Test]
		public void WhenCallingBeginTwice_WeSimplyReturn_Also_TestForRollbackedState()
		{
			using (var tx = new FileTransaction())
			{
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.NoTransaction));
				tx.Begin();
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.Active));
				Assert.That(tx.DistributedTransaction, Is.False);
				tx.Begin();
				Assert.That(tx.DistributedTransaction, Is.False, "Starting the same transaction twice should make no difference.");
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.Active));
				tx.Commit();
				Assert.That(tx.Status, Is.EqualTo(TransactionStatus.Committed));
			}
		}

		/*
		[Test, Explicit("Integration test rather than unit test.")]
		public void RecursiveDirSearch()
		{
			using (var t = new FileTransaction())
			{
				t.Begin();
				int folders;
				int files;
				RecurseDirectory(dllPath, 5, out files, out folders);
				Console.Write(string.Format("Total size: {2}, Files: {0}, Dirs: {1}", files, folders, size));
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern SafeFileHandle CreateFileW(
			[MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			NativeFileAccess dwDesiredAccess,
			NativeFileShare dwShareMode,
			IntPtr lpSecurityAttributes,
			NativeFileMode dwCreationDisposition,
			uint dwFlagsAndAttributes,
			SafeFileHandle hTemplateFile);
		
		// not tx
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern SafeFindHandle FindFirstFile(string lpFileName, 
		                                           out WIN32_FIND_DATA lpFindFileData);

		// not tx.
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern bool FindNextFile(SafeHandle hFindFile, 
		                                        out WIN32_FIND_DATA lpFindFileData);

		// Example
		public long RecurseDirectory(string directory, int level, out int files, out int folders)
		{
			IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
			long size = 0;
			files = 0;
			folders = 0;
			WIN32_FIND_DATA findData;

			// please note that the following line won't work if you try this on a network folder, like \\Machine\C$
			// simply remove the \\?\ part in this case or use \\?\UNC\ prefix
			using (SafeFindHandle findHandle = FindFirstFile(@"\\?\" + directory + @"\*", out findData))
			{
				if (!findHandle.IsInvalid)
				{
					do
					{
						if ((findData.dwFileAttributes & (uint)FileAttributes.Directory) != 0)
						{

							if (findData.cFileName != "." && findData.cFileName != "..")
							{
								folders++;

								int subfiles, subfolders;
								string subdirectory = directory + (directory.EndsWith(@"\") ? "" : @"\") +
								                      findData.cFileName;
								if (level != 0)  // allows -1 to do complete search.
								{
									size += RecurseDirectory(subdirectory, level - 1, out subfiles, out subfolders);

									folders += subfolders;
									files += subfiles;
								}
							}
						}
						else
						{
							// File
							files++;

							size += (long)findData.nFileSizeLow + (long)findData.nFileSizeHigh * 4294967296;
						}
					}
					while (FindNextFile(findHandle, out findData));
				}

			}

			return size;
		}
		*/
	}

	public static class Exs
	{
		/// <summary>
		/// Combines an input path and a path together
		/// using <see cref="System.IO.Path.Combine"/> and returns the result.
		/// </summary>
		public static string Combine(this string input, string path)
		{
			return System.IO.Path.Combine(input, path);
		}

		/// <summary>
		/// Combines two paths and makes sure the 
		/// DIRECTORY resulting from the combination exists
		/// by creating it with default permissions if it doesn't.
		/// </summary>
		/// <param name="input">The path to combine the latter with.</param>
		/// <param name="path">The latter path.</param>
		/// <returns>The combined path string.</returns>
		public static string CombineAssert(this string input, string path)
		{
			string p = input.Combine(path);

			if (!Directory.Exists(p))
				Directory.CreateDirectory(p);

			return p;
		}
	}
}