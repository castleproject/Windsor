#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;
using Castle.Services.vNextTransaction;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	[Explicit]
	public class ScratchBoard_LTM
	{
		private static readonly Random r = new Random((int) DateTime.Now.Ticks);

		/*
CREATE TABLE [dbo].[Thing] (
	[Id] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Val] [float] NOT NULL,
	CONSTRAINT [PK_Thing] PRIMARY KEY CLUSTERED ( [Id] ASC ) 
) ON [PRIMARY] 
		 */

		[SetUp]
		public void SetUp()
		{
			using (var c = GetConnection())
			using (var t = c.BeginTransaction())
			{
				using (var cmd = c.CreateCommand())
				{
					cmd.Transaction = t;
					cmd.CommandText = "truncate table Thing";
					cmd.ExecuteNonQuery();
				}

				using (var cmd = c.CreateCommand())
				{
					cmd.Transaction = t;
					cmd.CommandText = "insert into Thing values (@guid, @val)";
					cmd.Parameters.AddWithValue("guid", Guid.NewGuid());
					cmd.Parameters.AddWithValue("val", r.NextDouble());
					cmd.ExecuteNonQuery();
				}

				t.Commit();
			}
		}

		[Test]
		public void ImplicitScope()
		{
			Console.WriteLine("TEST STARTING");
			//Console.WriteLine("1 tx: {0}", System.Transactions.Transaction.Current););
			using (var ts = new TransactionScope())
			{
				using (var c = GetConnection())
				using (var cmd = c.CreateCommand())
				{
					//Console.WriteLine("2 tx: {0}", System.Transactions.Transaction.Current);
					cmd.CommandText = "SELECT TOP 1 Val FROM Thing";
					cmd.ExecuteScalar();
				}

				Console.WriteLine("COMPLETING");
				ts.Complete();
				//Console.WriteLine("3 tx: {0}", System.Transactions.Transaction.Current); // throws InvalidOperationException, "we've going to complete..."
				Console.WriteLine("DISPOSING");
			}
			//Console.WriteLine("4 tx: {0}", System.Transactions.Transaction.Current);
		}

		[Test]
		public void ExplicitTransaction()
		{
			using (var t = new CommittableTransaction())
			using (new TxScope(t))
			{
				using (var c = GetConnection())
				using (var cmd = c.CreateCommand())
				{
					cmd.CommandText = "SELECT TOP 1 Val FROM Thing";
					var scalar = (double) cmd.ExecuteScalar();
					Console.WriteLine("got val {0}", scalar);
				}
				Console.WriteLine("COMMITTING");
				t.Commit();
				Console.WriteLine("DISPOSING");
			}
		}

		[Test]
		public void ExplicitTransactionWithDependentTransaction()
		{
			using (var t = new CommittableTransaction())
			using (new TxScope(t))
			{
				Console.WriteLine("T1 STATUS: {0}", t.TransactionInformation.Status);

				using (var c = GetConnection())
				using (var cmd = c.CreateCommand())
				{
					cmd.CommandText = "SELECT TOP 1 Val FROM Thing";
					var scalar = (double) cmd.ExecuteScalar();
					Console.WriteLine("T1 STATUS: {0}", t.TransactionInformation.Status);
					Console.WriteLine("got val {0}, disposing command and connection", scalar);
				}

				using (var t2 = t.DependentClone(DependentCloneOption.RollbackIfNotComplete))
				using (new TxScope(t2))
				using (var c = GetConnection())
				using (var cmd = c.CreateCommand())
				{
					t2.TransactionCompleted +=
						(s, ea) =>
						Console.WriteLine("::: T2 TransactionCompleted: {0}", ea.Transaction.TransactionInformation.LocalIdentifier);
					cmd.CommandText = "DELETE FROM Thing";
					Console.WriteLine("T2: EXECUTING NON QUERY");
					cmd.ExecuteNonQuery();

					Console.WriteLine("T2: Enlisting volatile");
					t2.EnlistVolatile(new VolatileResource(false), EnlistmentOptions.None);

					Console.WriteLine("T2: COMPLETE-CALL");
					t2.Complete();
					Console.WriteLine("T2 STATUS: {0}", t2.TransactionInformation.Status);
				}

				Console.WriteLine("T1: COMMITTING, status: {0}", t.TransactionInformation.Status);
				try
				{
					t.Commit();
				}
				catch (TransactionAbortedException e)
				{
					Console.WriteLine("TransactionAbortedException, {0}", e);
					t.Rollback(e);
				}
				Console.WriteLine("T1 STATUS: {0}", t.TransactionInformation.Status);
				Console.WriteLine("T1: DISPOSING");
			}
		}

		[Test, Explicit("This test will fail because of how System.Transactions work. I can't roll back a transaction if a resource failed.")]
		public void RetryOnFailure()
		{
			using (var t = new CommittableTransaction())
			using (new TxScope(t))
			{
				t.EnlistVolatile(new ThrowingResource(true), EnlistmentOptions.EnlistDuringPrepareRequired);

				using (var c = GetConnection())
				using (var cmd = c.CreateCommand())
				{
					cmd.CommandText = "SELECT TOP 1 Val FROM Thing";
					var scalar = (double)cmd.ExecuteScalar();
					Console.WriteLine("got val {0}", scalar);
				}

				try
				{
					t.Commit();
					Assert.Fail("first commit should fail");
				}
				catch (ApplicationException)
				{
					Assert.That(t.TransactionInformation.Status, Is.EqualTo(System.Transactions.TransactionStatus.Committed));
				}
			}
			
		}

		private SqlConnection GetConnection()
		{
			Console.WriteLine("CREATE CONNECTION");
			var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["test"].ConnectionString);
			Console.WriteLine("OPEN CONNECTION");
			connection.Open();
			return connection;
		}
	}

	public class VolatileResource : ISinglePhaseNotification
	{
		private readonly bool _ThrowIt;
		private int _ErrorCount = 0;

		public VolatileResource(bool throwIt)
		{
			_ThrowIt = throwIt;
		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			Console.WriteLine("PREPARE - VolatileResource");
			if (_ThrowIt && ++_ErrorCount <2) throw new ApplicationException("simulating resource failure");

			preparingEnlistment.ForceRollback();
		}

		public void Commit(Enlistment enlistment)
		{
			Console.WriteLine("COMMIT - VolatileResource");
			enlistment.Done();
		}

		public void Rollback(Enlistment enlistment)
		{
			Console.WriteLine("ROLLBACK - VolatileResource");
			enlistment.Done();
		}

		public void InDoubt(Enlistment enlistment)
		{
			Console.WriteLine("INDOUBT - VolatileResource");
			enlistment.Done();
		}

		/// <summary>
		/// 	Represents the resource manager's implementation of the callback for the single phase commit optimization.
		/// </summary>
		/// <param name = "singlePhaseEnlistment">A <see cref = "T:System.Transactions.SinglePhaseEnlistment" />  used to send a response to the transaction manager.</param>
		public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
		{
			Console.WriteLine("SinglePhaseCommit");
			singlePhaseEnlistment.Committed();
		}
	}
}