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

namespace Castle.Facilities.NHibernateIntegration
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Internal;
	using MicroKernel;
	using MicroKernel.Facilities;
	using NHibernate;
	using Services.Transaction;
	using ITransaction = Services.Transaction.ITransaction;

	/// <summary>
	/// 
	/// </summary>
	public class DefaultSessionManager : MarshalByRefObject, ISessionManager
	{
		#region constants

		/// <summary>
		/// Format string for NHibernate interceptor components
		/// </summary>
		public const string InterceptorFormatString = "nhibernate.session.interceptor.{0}";

		/// <summary>
		/// Name for NHibernate Interceptor componentInterceptorName
		/// </summary>
		public const string InterceptorName = "nhibernate.session.interceptor";

		#endregion

		private readonly IKernel kernel;
		private readonly ISessionStore sessionStore;
		private readonly ISessionFactoryResolver factoryResolver;
		private FlushMode defaultFlushMode = FlushMode.Auto;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultSessionManager"/> class.
		/// </summary>
		/// <param name="sessionStore">The session store.</param>
		/// <param name="kernel">The kernel.</param>
		/// <param name="factoryResolver">The factory resolver.</param>
		public DefaultSessionManager(ISessionStore sessionStore, IKernel kernel, ISessionFactoryResolver factoryResolver)
		{
			this.kernel = kernel;
			this.sessionStore = sessionStore;
			this.factoryResolver = factoryResolver;
		}

		/// <summary>
		/// The flushmode the created session gets
		/// </summary>
		/// <value></value>
		public FlushMode DefaultFlushMode
		{
			get { return defaultFlushMode; }
			set { defaultFlushMode = value; }
		}

		/// <summary>
		/// Returns a valid opened and connected ISession instance.
		/// </summary>
		/// <returns></returns>
		public ISession OpenSession()
		{
			return OpenSession(Constants.DefaultAlias);
		}

		/// <summary>
		/// Returns a valid opened and connected ISession instance
		/// for the given connection alias.
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		public ISession OpenSession(String alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");

			ITransaction transaction = ObtainCurrentTransaction();

			SessionDelegate wrapped = sessionStore.FindCompatibleSession(alias);

			ISession session;

			if (wrapped == null)
			{
				session = CreateSession(alias);

				wrapped = WrapSession(transaction != null, session);
				EnlistIfNecessary(true, transaction, wrapped);
				sessionStore.Store(alias, wrapped);
			}
			else
			{
				EnlistIfNecessary(false, transaction, wrapped);
				wrapped = WrapSession(true, wrapped.InnerSession);
			}

			return wrapped;
		}

		/// <summary>
		/// Returns a valid opened and connected IStatelessSession instance
		/// </summary>
		/// <returns></returns>
		public IStatelessSession OpenStatelessSession()
		{
			return this.OpenStatelessSession(Constants.DefaultAlias);

		}

		/// <summary>
		/// Returns a valid opened and connected IStatelessSession instance
		/// for the given connection alias.
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		public IStatelessSession OpenStatelessSession(String alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");

			ITransaction transaction = ObtainCurrentTransaction();

			StatelessSessionDelegate wrapped = sessionStore.FindCompatibleStatelessSession(alias);

			IStatelessSession session;

			if (wrapped == null)
			{
				session = CreateStatelessSession(alias);

				wrapped = WrapSession(transaction != null, session);
				EnlistIfNecessary(true, transaction, wrapped);
				sessionStore.Store(alias, wrapped);
			}
			else
			{
				EnlistIfNecessary(false, transaction, wrapped);
				wrapped = WrapSession(true, wrapped.InnerSession);
			}

			return wrapped;
		}

		/// <summary>
		/// Enlists if necessary.
		/// </summary>
		/// <param name="weAreSessionOwner">if set to <c>true</c> [we are session owner].</param>
		/// <param name="transaction">The transaction.</param>
		/// <param name="session">The session.</param>
		/// <returns></returns>
		protected bool EnlistIfNecessary(bool weAreSessionOwner,
										 ITransaction transaction,
										 SessionDelegate session)
		{
			if (transaction == null) return false;

			var list = (IList<ISession>) transaction.Context["nh.session.enlisted"];

			bool shouldEnlist;

			if (list == null)
			{
				list = new List<ISession>();

				shouldEnlist = true;
			}
			else
			{
				shouldEnlist = true;

				foreach (ISession sess in list)
				{
					if (SessionDelegate.AreEqual(session, sess))
					{
						shouldEnlist = false;
						break;
					}
				}
			}

			if (shouldEnlist)
			{
				if (session.Transaction == null || !session.Transaction.IsActive)
				{
					transaction.Context["nh.session.enlisted"] = list;

					IsolationLevel level = TranslateIsolationLevel(transaction.IsolationMode);
					transaction.Enlist(new ResourceAdapter(session.BeginTransaction(level), transaction.IsAmbient));

					list.Add(session);
				}

				if (weAreSessionOwner)
				{
					transaction.RegisterSynchronization(new SessionDisposeSynchronization(session));
				}
			}

			return true;
		}

		/// <summary>
		/// Enlists if necessary.
		/// </summary>
		/// <param name="weAreSessionOwner">if set to <c>true</c> [we are session owner].</param>
		/// <param name="transaction">The transaction.</param>
		/// <param name="session">The session.</param>
		/// <returns></returns>
		protected bool EnlistIfNecessary(bool weAreSessionOwner,
										 ITransaction transaction,
										 StatelessSessionDelegate session)
		{
			if (transaction == null) return false;

			var list = (IList<IStatelessSession>) transaction.Context["nh.statelessSession.enlisted"];

			bool shouldEnlist;

			if (list == null)
			{
				list = new List<IStatelessSession>();

				shouldEnlist = true;
			}
			else
			{
				shouldEnlist = true;

				foreach (IStatelessSession sess in list)
				{
					if (StatelessSessionDelegate.AreEqual(session, sess))
					{
						shouldEnlist = false;
						break;
					}
				}
			}

			if (shouldEnlist)
			{
				if (session.Transaction == null || !session.Transaction.IsActive)
				{
					transaction.Context["nh.statelessSession.enlisted"] = list;

					IsolationLevel level = TranslateIsolationLevel(transaction.IsolationMode);
					transaction.Enlist(new ResourceAdapter(session.BeginTransaction(level), transaction.IsAmbient));

					list.Add(session);
				}

				if (weAreSessionOwner)
				{
					transaction.RegisterSynchronization(new StatelessSessionDisposeSynchronization(session));
				}
			}

			return true;
		}

		private static IsolationLevel TranslateIsolationLevel(IsolationMode mode)
		{
			switch (mode)
			{
				case IsolationMode.Chaos:
					return IsolationLevel.Chaos;
				case IsolationMode.ReadCommitted:
					return IsolationLevel.ReadCommitted;
				case IsolationMode.ReadUncommitted:
					return IsolationLevel.ReadUncommitted;
				case IsolationMode.RepeatableRead:
					return IsolationLevel.RepeatableRead;
				case IsolationMode.Serializable:
					return IsolationLevel.Serializable;
				default:
					return IsolationLevel.Unspecified;
			}
		}

		private ITransaction ObtainCurrentTransaction()
		{
			ITransactionManager transactionManager = kernel.Resolve<ITransactionManager>();

			return transactionManager.CurrentTransaction;
		}

		private SessionDelegate WrapSession(bool hasTransaction, ISession session)
		{
			return new SessionDelegate(!hasTransaction, session, sessionStore);
		}

		private StatelessSessionDelegate WrapSession(bool hasTransaction, IStatelessSession session)
		{
			return new StatelessSessionDelegate(!hasTransaction, session, sessionStore);
		}
		
		private ISession CreateSession(String alias)
		{
			ISessionFactory sessionFactory = factoryResolver.GetSessionFactory(alias);

			if (sessionFactory == null)
			{
				throw new FacilityException("No ISessionFactory implementation " +
											"associated with the given alias: " + alias);
			}

			ISession session;

			string aliasedInterceptorId = string.Format(InterceptorFormatString, alias);

			if (kernel.HasComponent(aliasedInterceptorId))
			{
				IInterceptor interceptor = kernel.Resolve<IInterceptor>(aliasedInterceptorId);

				session = sessionFactory.OpenSession(interceptor);
			}
			else if (kernel.HasComponent(InterceptorName))
			{
				IInterceptor interceptor = kernel.Resolve<IInterceptor>(InterceptorName);

				session = sessionFactory.OpenSession(interceptor);
			}
			else
			{
				session = sessionFactory.OpenSession();
			}

			session.FlushMode = defaultFlushMode;

			return session;
		}

		private IStatelessSession CreateStatelessSession(String alias)
		{
			ISessionFactory sessionFactory = factoryResolver.GetSessionFactory(alias);

			if (sessionFactory == null)
			{
				throw new FacilityException("No ISessionFactory implementation " +
											"associated with the given alias: " + alias);
			}

			IStatelessSession session = sessionFactory.OpenStatelessSession();

			return session;
		}
	}
}