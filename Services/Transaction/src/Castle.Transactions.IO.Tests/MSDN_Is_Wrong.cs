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

namespace Castle.Transactions.IO.Tests
{
	using System;
	using System.IO;
	using System.Threading;

	using Castle.Transactions.Activities;

	using NUnit.Framework;

	public class MSDN_Is_Wrong
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

		// http://msdn.microsoft.com/en-us/library/aa365536%28VS.85%29.aspx
		[Test]
		[Explicit(
			"MSDN is wrong in saying: \"If a non-transacted thread modifies the file before the transacted thread does, "
			+ "and the file is still open when the transaction attempts to open it, "
			+ "the transaction receives the error ERROR_TRANSACTIONAL_CONFLICT.\"... "
			+ "This test proves the error in this statement. Actually, from testing the rest of the code, it's clear that "
			+ "the error comes for the opposite; when a transacted thread modifies before a non-transacted thread.")]
		public void TwoTransactions_SameName_FirstSleeps()
		{
			var t1_started = new ManualResetEvent(false);
			var t2_started = new ManualResetEvent(false);
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
						Console.WriteLine("t2 wait for t1 to start");
						Console.Out.Flush();
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
					Console.WriteLine("t2 finally");
					Console.Out.Flush();
					t2_started.Set();
				}
			});

			t1.Start();

			using (IFileTransaction t = subject.CreateFileTransaction().Value)
			{
				Console.WriteLine("t1 wait for t2 to start");
				Console.Out.Flush();
				t2_started.WaitOne();

				try
				{
					Console.WriteLine("t1 started");
					// the transacted thread should receive ERROR_TRANSACTIONAL_CONFLICT, but it gets permission denied.
					//using (var fs = ((IFileAdapter)t).Create("abb"))
					//    fs.WriteByte(0x2);
				}
				finally
				{
					Console.WriteLine("t1 finally");
					Console.Out.Flush();
					t1_started.Set();
				}

				t.Complete();
			}

			if (e != null)
			{
				Console.WriteLine(e);
				Assert.Fail(e.Message);
			}
		}
	}
}