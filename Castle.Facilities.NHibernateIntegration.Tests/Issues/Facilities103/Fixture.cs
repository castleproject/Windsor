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

namespace Castle.Facilities.NHibernateIntegration.Tests.Issues.Facilities103
{
	using System;
	using System.Collections;
	using System.Data;
	using MicroKernel;
	using NHibernate;
	using NUnit.Framework;
	using Rhino.Mocks;
	using Services.Transaction;
	using SessionStores;
	using ITransaction = Services.Transaction.ITransaction;

	[TestFixture]
	public class DefaultSessionManagerTestCase : IssueTestCase
	{
		protected override string ConfigurationFile
		{
			get { return "EmptyConfiguration.xml"; }
		}

		public override void OnSetUp()
		{
			sessionStore = new CallContextSessionStore();
			kernel = mockRepository.DynamicMock<IKernel>();
			factoryResolver = mockRepository.DynamicMock<ISessionFactoryResolver>();
			transactionManager = mockRepository.DynamicMock<ITransactionManager>();
			transaction = mockRepository.DynamicMock<ITransaction>();
			sessionFactory = mockRepository.DynamicMock<ISessionFactory>();
			session = mockRepository.DynamicMock<ISession>();
			statelessSession = mockRepository.DynamicMock<IStatelessSession>();
			contextDictionary = new Hashtable();
			sessionManager = new DefaultSessionManager(sessionStore, kernel, factoryResolver);
		}

		private const string Alias = "myAlias";
		private const string InterceptorFormatString = DefaultSessionManager.InterceptorFormatString;
		private const string InterceptorName = DefaultSessionManager.InterceptorName;
		private const IsolationMode DefaultIsolationMode = IsolationMode.ReadUncommitted;
		private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadUncommitted;

		#region mock variables

		private ISessionStore sessionStore;
		private IKernel kernel;
		private ISessionFactoryResolver factoryResolver;
		private ITransactionManager transactionManager;
		private ITransaction transaction;
		private ISessionFactory sessionFactory;
		private ISession session;
		private IStatelessSession statelessSession;
		private IDictionary contextDictionary;
		private ISessionManager sessionManager;

		#endregion

		[Test]
		public void WhenBeginTransactionFailsSessionIsRemovedFromSessionStore()
		{
			using (mockRepository.Record())
			{
				Expect.Call(kernel.Resolve<ITransactionManager>()).Return(transactionManager);
				Expect.Call(transactionManager.CurrentTransaction).Return(transaction);
				Expect.Call(factoryResolver.GetSessionFactory(Alias)).Return(sessionFactory);
				Expect.Call(kernel.HasComponent(string.Format(InterceptorFormatString, Alias))).Return(false);
				Expect.Call(kernel.HasComponent(InterceptorName)).Return(false).Repeat.Any();
				Expect.Call(sessionFactory.OpenSession()).Return(session);
				session.FlushMode = sessionManager.DefaultFlushMode;
				Expect.Call(transaction.Context).Return(contextDictionary).Repeat.Any();
				Expect.Call(transaction.IsolationMode).Return(DefaultIsolationMode).Repeat.Any();
				Expect.Call(session.BeginTransaction(DefaultIsolationLevel)).Throw(new Exception());
			}

			using (mockRepository.Playback())
			{
				try
				{
					sessionManager.OpenSession(Alias);
					Assert.Fail("DbException not thrown");
				}
				catch (Exception)
				{
					//ignore
					//Console.WriteLine(ex.ToString());
				}
				Assert.IsNull(sessionStore.FindCompatibleSession(Alias),
				              "The sessionStore shouldn't contain compatible session if the session creation fails");
			}
		}

		[Test]
		public void WhenBeginTransactionFailsStatelessSessionIsRemovedFromSessionStore()
		{
			using (mockRepository.Record())
			{
				Expect.Call(kernel.Resolve<ITransactionManager>()).Return(transactionManager);
				Expect.Call(transactionManager.CurrentTransaction).Return(transaction);
				Expect.Call(factoryResolver.GetSessionFactory(Alias)).Return(sessionFactory);
				Expect.Call(sessionFactory.OpenStatelessSession()).Return(statelessSession);
				Expect.Call(transaction.Context).Return(contextDictionary).Repeat.Any();
				// TODO: NHibernate doesn't support IStatelessSession.BeginTransaction(IsolationLevel) yet.
				////Expect.Call(transaction.IsolationMode).Return(DefaultIsolationMode).Repeat.Any();
				////Expect.Call(statelessSession.BeginTransaction(DefaultIsolationMode)).Throw(new Exception());
				Expect.Call(statelessSession.BeginTransaction()).Throw(new Exception());
			}

			using (mockRepository.Playback())
			{
				try
				{
					sessionManager.OpenStatelessSession(Alias);
					Assert.Fail("DbException not thrown");
				}
				catch (Exception)
				{
					//ignore
					//Console.WriteLine(ex.ToString());
				}
				Assert.IsNull(
					sessionStore.FindCompatibleStatelessSession(Alias),
					"The sessionStore shouldn't contain compatible session if the session creation fails");
			}
		}
	}
}