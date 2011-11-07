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

namespace Castle.Facilities.NHibernateIntegration.Tests.Internals
{
	using System;
	using Common;
	using MicroKernel.Facilities;
	using NHibernate;
	using NUnit.Framework;
	using Services.Transaction;
	using ITransaction = Services.Transaction.ITransaction;

	/// <summary>
	/// Tests the default implementation of ISessionStore
	/// </summary>
	[TestFixture]
	public class SessionManagerTestCase : AbstractNHibernateTestCase
	{
		protected override string ConfigurationFile
		{
			get { return "Internals/TwoDatabaseConfiguration.xml"; }
		}

		[Test]
		public void TwoDatabases()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ISession session1 = manager.OpenSession();
			ISession session2 = manager.OpenSession("db2");

			Assert.IsNotNull(session1);
			Assert.IsNotNull(session2);

			Assert.IsFalse(Object.ReferenceEquals(session1, session2));

			session2.Dispose();
			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		[Test]
		public void NonInterceptedSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			string sessionAlias = "db2";

			ISession session = manager.OpenSession(sessionAlias);
			Order o = new Order();
			o.Value = 9.3f;
			session.SaveOrUpdate(o);
			session.Close();

			session = manager.OpenSession(sessionAlias);
			session.Get(typeof (Order), 1);
			session.Close();

			TestInterceptor interceptor = container.Resolve<TestInterceptor>("nhibernate.session.interceptor.intercepted");
			Assert.IsNotNull(interceptor);
			Assert.IsFalse(interceptor.ConfirmOnSaveCall());
			Assert.IsFalse(interceptor.ConfirmInstantiationCall());
			interceptor.ResetState();
		}

		[Test]
		public void InterceptedSessionByConfiguration()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			string sessionAlias = "intercepted";

			ISession session = manager.OpenSession(sessionAlias);
			Order o = new Order();
			o.Value = 9.3f;
			session.SaveOrUpdate(o);
			session.Close();

			session = manager.OpenSession(sessionAlias);
			session.Get(typeof (Order), 1);
			session.Close();

			TestInterceptor interceptor = container.Resolve<TestInterceptor>("nhibernate.session.interceptor.intercepted");
			Assert.IsNotNull(interceptor);
			Assert.IsTrue(interceptor.ConfirmOnSaveCall());
			Assert.IsTrue(interceptor.ConfirmInstantiationCall());
			interceptor.ResetState();
		}

		[Test]
		public void NonExistentAlias()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			Assert.Throws<FacilityException>(() => manager.OpenSession("something in the way she moves"));
		}

		[Test]
		public void SharedSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ISession session1 = manager.OpenSession();
			ISession session2 = manager.OpenSession();
			ISession session3 = manager.OpenSession();

			Assert.IsNotNull(session1);
			Assert.IsNotNull(session2);
			Assert.IsNotNull(session3);

			Assert.IsTrue(SessionDelegate.AreEqual(session1, session2));
			Assert.IsTrue(SessionDelegate.AreEqual(session1, session3));

			session3.Dispose();
			session2.Dispose();
			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// This test ensures that the transaction takes 
		/// ownership of the session and disposes it at the end
		/// of the transaction
		/// </summary>
		[Test]
		// [Ignore("This doesn't work with the NH 1.2 transaction property, needs to be fixed")]
		public void NewTransactionBeforeUsingSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires, IsolationMode.Serializable);

			transaction.Begin();

			ISession session = manager.OpenSession();

			Assert.IsNotNull(session);
			Assert.IsNotNull(session.Transaction);

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session.Transaction.WasCommitted);
			// Assert.IsTrue(session.IsConnected); 

			session.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// In this case the transaction should not take
		/// ownership of the session (not dipose it at the 
		/// end of the transaction)
		/// </summary>
		[Test]
		// [Ignore("This doesn't work with the NH 1.2 transaction property, needs to be fixed")]
		public void NewTransactionAfterUsingSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ISession session1 = manager.OpenSession();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires, IsolationMode.Serializable);

			transaction.Begin();

			// Nested			
			using (ISession session2 = manager.OpenSession())
			{
				Assert.IsNotNull(session2);

				Assert.IsNotNull(session1);
				Assert.IsNotNull(session1.Transaction,
				                 "After requesting compatible session, first session is enlisted in transaction too.");
				Assert.IsTrue(session1.Transaction.IsActive,
				              "After requesting compatible session, first session is enlisted in transaction too.");

				using (ISession session3 = manager.OpenSession())
				{
					Assert.IsNotNull(session3);
					Assert.IsNotNull(session3.Transaction);
					Assert.IsTrue(session3.Transaction.IsActive);
				}

				SessionDelegate delagate1 = (SessionDelegate) session1;
				SessionDelegate delagate2 = (SessionDelegate) session2;
				Assert.AreSame(delagate1.InnerSession, delagate2.InnerSession);
			}

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session1.Transaction.WasCommitted);
			Assert.IsTrue(session1.IsConnected);

			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// This test ensures that the transaction enlists the 
		/// the sessions of both database connections
		/// </summary>
		[Test]
		//[Ignore("This doesn't work with the NH 1.2 transaction property, needs to be fixed")]
		public void NewTransactionBeforeUsingSessionWithTwoDatabases()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires, IsolationMode.Serializable);

			transaction.Begin();

			ISession session1 = manager.OpenSession();
			Assert.IsNotNull(session1);
			Assert.IsNotNull(session1.Transaction);

			ISession session2 = manager.OpenSession("db2");
			Assert.IsNotNull(session2);
			Assert.IsNotNull(session2.Transaction);

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session1.Transaction.WasCommitted);
			// Assert.IsTrue(session1.IsConnected);
			// TODO: Assert transaction was committed
			// Assert.IsTrue(session2.Transaction.WasCommitted);
			// Assert.IsTrue(session2.IsConnected);

			session2.Dispose();
			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// This test ensures that the session is enlisted in actual transaction 
		/// only once for second database session
		/// </summary>
		[Test]
		//[Ignore("This doesn't work with the NH 1.2 transaction property, needs to be fixed")]
		public void SecondDatabaseSessionEnlistedOnlyOnceInActualTransaction()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires, IsolationMode.Serializable);

			transaction.Begin();

			// open connection to first database and enlist session in running transaction
			ISession session1 = manager.OpenSession();

			// open connection to second database and enlist session in running transaction
			using (ISession session2 = manager.OpenSession("db2"))
			{
				Assert.IsNotNull(session2);
				Assert.IsNotNull(session2.Transaction);
			}
			// "real" NH session2 was not disposed because its in active transaction

			// request compatible session for db2 --> we must get existing NH session to db2 which should be already enlisted in active transaction
			using (ISession session3 = manager.OpenSession("db2"))
			{
				Assert.IsNotNull(session3);
				Assert.IsTrue(session3.Transaction.IsActive);
			}

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session1.Transaction.WasCommitted);
			// Assert.IsTrue(session1.IsConnected); 

			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		[Test]
		public void TwoDatabasesStateless()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			IStatelessSession session1 = manager.OpenStatelessSession();
			IStatelessSession session2 = manager.OpenStatelessSession("db2");

			Assert.IsNotNull(session1);
			Assert.IsNotNull(session2);

			Assert.IsFalse(Object.ReferenceEquals(session1, session2));

			session2.Dispose();
			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		[Test]
		public void NonExistentAliasStateless()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			Assert.Throws<FacilityException>(() => manager.OpenStatelessSession("something in the way she moves"));
		}

		[Test]
		public void SharedStatelessSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			IStatelessSession session1 = manager.OpenStatelessSession();
			IStatelessSession session2 = manager.OpenStatelessSession();
			IStatelessSession session3 = manager.OpenStatelessSession();

			Assert.IsNotNull(session1);
			Assert.IsNotNull(session2);
			Assert.IsNotNull(session3);

			Assert.IsTrue(StatelessSessionDelegate.AreEqual(session1, session2));
			Assert.IsTrue(StatelessSessionDelegate.AreEqual(session1, session3));

			session3.Dispose();
			session2.Dispose();
			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// This test ensures that the transaction takes 
		/// ownership of the session and disposes it at the end
		/// of the transaction
		/// </summary>
		[Test]
		public void NewTransactionBeforeUsingStatelessSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires,
				IsolationMode.Serializable);

			transaction.Begin();

			IStatelessSession session = manager.OpenStatelessSession();

			Assert.IsNotNull(session);
			Assert.IsNotNull(session.Transaction);

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session.Transaction.WasCommitted);
			// Assert.IsTrue(session.IsConnected); 

			session.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// In this case the transaction should not take
		/// ownership of the session (not dipose it at the 
		/// end of the transaction)
		/// </summary>
		[Test]
		public void NewTransactionAfterUsingStatelessSession()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			IStatelessSession session1 = manager.OpenStatelessSession();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires,
				IsolationMode.Serializable);

			transaction.Begin();

			// Nested			
			using (IStatelessSession session2 = manager.OpenStatelessSession())
			{
				Assert.IsNotNull(session2);

				Assert.IsNotNull(session1);
				Assert.IsNotNull(session1.Transaction,
								 "After requesting compatible session, first session is enlisted in transaction too.");
				Assert.IsTrue(session1.Transaction.IsActive,
							  "After requesting compatible session, first session is enlisted in transaction too.");

				using (ISession session3 = manager.OpenSession())
				{
					Assert.IsNotNull(session3);
					Assert.IsNotNull(session3.Transaction);
					Assert.IsTrue(session3.Transaction.IsActive);
				}

				StatelessSessionDelegate delagate1 = (StatelessSessionDelegate) session1;
				StatelessSessionDelegate delagate2 = (StatelessSessionDelegate) session2;
				Assert.AreSame(delagate1.InnerSession, delagate2.InnerSession);
			}

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session1.Transaction.WasCommitted);
			Assert.IsTrue(session1.IsConnected);

			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// This test ensures that the transaction enlists the 
		/// the sessions of both database connections
		/// </summary>
		[Test]
		public void NewTransactionBeforeUsingStatelessSessionWithTwoDatabases()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires,
				IsolationMode.Serializable);

			transaction.Begin();

			IStatelessSession session1 = manager.OpenStatelessSession();
			Assert.IsNotNull(session1);
			Assert.IsNotNull(session1.Transaction);

			IStatelessSession session2 = manager.OpenStatelessSession("db2");
			Assert.IsNotNull(session2);
			Assert.IsNotNull(session2.Transaction);

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session1.Transaction.WasCommitted);
			// Assert.IsTrue(session1.IsConnected);
			// TODO: Assert transaction was committed
			// Assert.IsTrue(session2.Transaction.WasCommitted);
			// Assert.IsTrue(session2.IsConnected);

			session2.Dispose();
			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}

		/// <summary>
		/// This test ensures that the session is enlisted in actual transaction 
		/// only once for second database session
		/// </summary>
		[Test]
		public void SecondDatabaseStatelessSessionEnlistedOnlyOnceInActualTransaction()
		{
			ISessionManager manager = container.Resolve<ISessionManager>();

			ITransactionManager tmanager = container.Resolve<ITransactionManager>();

			ITransaction transaction = tmanager.CreateTransaction(
				TransactionMode.Requires,
				IsolationMode.Serializable);

			transaction.Begin();

			// open connection to first database and enlist session in running transaction
			IStatelessSession session1 = manager.OpenStatelessSession();

			// open connection to second database and enlist session in running transaction
			using (IStatelessSession session2 = manager.OpenStatelessSession("db2"))
			{
				Assert.IsNotNull(session2);
				Assert.IsNotNull(session2.Transaction);
			}
			// "real" NH session2 was not disposed because its in active transaction

			// request compatible session for db2 --> we must get existing NH session to db2 which should be already enlisted in active transaction
			using (IStatelessSession session3 = manager.OpenStatelessSession("db2"))
			{
				Assert.IsNotNull(session3);
				Assert.IsTrue(session3.Transaction.IsActive);
			}

			transaction.Commit();

			// TODO: Assert transaction was committed
			// Assert.IsTrue(session1.Transaction.WasCommitted);
			// Assert.IsTrue(session1.IsConnected); 

			session1.Dispose();

			Assert.IsTrue(container.Resolve<ISessionStore>().IsCurrentActivityEmptyFor(Constants.DefaultAlias));
		}
	}
}