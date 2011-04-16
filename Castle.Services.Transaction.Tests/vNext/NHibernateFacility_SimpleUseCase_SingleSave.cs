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
using System.Diagnostics.Contracts;
using Castle.Facilities.NHibernate;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction.Lifestyles;
using Castle.Windsor;
using log4net;
using log4net.Config;
using NHibernate;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class NHibernateFacility_SimpleUseCase_SingleSave
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (NHibernateFacility_SimpleUseCase_SingleSave));

		private WindsorContainer c;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
			c = GetWindsorContainer();
		}

		[TearDown]
		public void TearDown()
		{
			_Logger.Debug("running tear-down, removing components");

			c.Register(Component.For<TearDownService>().LifeStyle.HybridPerTransactionTransient());
			using (var s = new ResolveScope<TearDownService>(c))
				s.Service.ClearThings();

			c.Dispose();
		}

		[Test]
		public void SavingWith_Transient_Lifestyle()
		{
			// in your app_start:
			c.Register(Component.For<ServiceUsingTransientSessionLifestyle>().LifeStyle.HybridPerTransactionTransient());

			// your controller calls:
			using (var scope = new ResolveScope<ServiceUsingTransientSessionLifestyle>(c))
			{
				scope.Service.SaveNewThing();
				Assert.That(scope.Service.VerifyThing(), Is.Not.Null);
			}
		}

		[Test]
		public void SavingWith_PerTransaction_Lifestyle()
		{
			c.Register(Component.For<ServiceUsingPerTransactionSessionLifestyle>().LifeStyle.HybridPerTransactionTransient());

			// given
			using (var scope = new ResolveScope<ServiceUsingPerTransactionSessionLifestyle>(c))
			{
				// then
				scope.Service.SaveNewThing();
				Assert.That(scope.Service.LoadNewThing(), Is.Not.Null, "because it was saved by the previous method call");
			}
		}

		private WindsorContainer GetWindsorContainer()
		{
			var c = new WindsorContainer();
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());
			c.AddFacility<NHibernateFacility>();
			return c;
		}
	}

	public class TearDownService
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TearDownService));

		private readonly Func<ISession> _Session;

		public TearDownService(Func<ISession> session)
		{
			Contract.Requires(session != null);
			_Session = session;
		}

		[vNextTransaction.Transaction]
		public virtual void ClearThings()
		{
			_Logger.Debug("clearing things");
			_Session().Delete("from Thing");
		}
	}

	public class ServiceUsingPerTransactionSessionLifestyle
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ServiceUsingPerTransactionSessionLifestyle));

		private readonly Func<ISession> _GetSession;
		private Guid _Id;

		public ServiceUsingPerTransactionSessionLifestyle(Func<ISession> getSession)
		{
			Contract.Requires(getSession != null);
			_GetSession = getSession;
		}

		// a bit of documentation
		/// <remarks>
		/// 	<para>This method and the next demonstrate how you COULD use the factory delegate.
		/// 		EITHER you run dispose on the ISession, or you don't. In fact, NHibernate
		/// 		will dispose the ISession after the transaction is complete.</para>
		/// 	<para>This is what the log looks like with this code:</para>
		/// 	<para>
		/// 		2109 [TestRunnerThread] DEBUG Castle.Services.Transaction.Tests.vNext.ServiceUsingPerTransactionSessionLifestyle (null) - exiting using-block of session
		/// 		2109 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - [session-id=c2f5673f-f93f-43c9-ad27-c7df8f33edc7] running ISession.Dispose()
		/// 		System.Transactions Information: 0 : TransactionScope Created: <TraceSource>[Base]</TraceSource><TransactionTraceIdentifier><TransactionIdentifier>f5568393-d069-4e2d-b85c-5f928f4e64c7:1</TransactionIdentifier><CloneIdentifier>2</CloneIdentifier></TransactionTraceIdentifier><TransactionScopeResult>TransactionPassed</TransactionScopeResult>
		/// 		System.Transactions Information: 0 : Dependent Clone Created: <TraceSource>[Lightweight]</TraceSource><TransactionTraceIdentifier><TransactionIdentifier>f5568393-d069-4e2d-b85c-5f928f4e64c7:1</TransactionIdentifier><CloneIdentifier>3</CloneIdentifier></TransactionTraceIdentifier><DependentCloneOption>RollbackIfNotComplete</DependentCloneOption>
		/// 		2111 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - before transaction completion
		/// 	</para>
		/// 	<para>
		/// 		As you can see, there's no disposing of the ISession but until here:
		/// 	</para>
		/// 	<para>
		/// 		2163 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - [session-id=c2f5673f-f93f-43c9-ad27-c7df8f33edc7] executing real Dispose(True)
		/// 		2164 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - closing session
		/// 		2164 [TestRunnerThread] DEBUG NHibernate.AdoNet.AbstractBatcher (null) - running BatcherImpl.Dispose(true)
		/// 		2168 [TestRunnerThread] DEBUG Castle.Services.vNextTransaction.NHibernate.PerTransactionLifestyleManagerBase (null) - transaction#f5568393-d069-4e2d-b85c-5f928f4e64c7:1 completed, disposing object 'NHibernate.Impl.SessionImpl'
		/// 		2168 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - [session-id=c2f5673f-f93f-43c9-ad27-c7df8f33edc7] running ISession.Dispose()
		/// 	</para>
		/// 	<para>
		/// 		It's impossible for the PerTransaction lifestyle to KNOW when the 'real' disposing of the ISession is, so it's still required to try and Release the component. However,
		/// 		this is not just to call Dispose on the ISession; it is also to let Windsor stop tracking the reference, which would otherwise lead to a memory leak.
		/// 	</para>
		/// </remarks>
		[vNextTransaction.Transaction]
		public virtual void SaveNewThing()
		{
			_Logger.DebugFormat("save new thing");

			using (var session = _GetSession())
			{
				// at KTH this is an arbitrary number
				_Id = (Guid) session.Save(new Thing(17.0));

				_Logger.DebugFormat("exiting using-block of session");
			}
		}

		[vNextTransaction.Transaction]
		public virtual Thing LoadNewThing()
		{
			// be aware how I'm not manually disposing the ISession here; I could, but it would make no difference
			return _GetSession().Get<Thing>(_Id);
		}
	}

	// this class uses the transient lifestyle because it resolves ISession in the constructor and
	// it's not resolving Func<ISession> or ISessionManager.
	public class ServiceUsingTransientSessionLifestyle
	{
		private readonly ISession _Session;
		private readonly IStatelessSession _StatelessSession;
		private Guid _ThingId;

		public ServiceUsingTransientSessionLifestyle(ISession session, IStatelessSession statelessSession)
		{
			Contract.Requires(session != null);
			_Session = session;
			_StatelessSession = statelessSession;
		}

		[vNextTransaction.Transaction]
		public virtual void SaveNewThing()
		{
			_ThingId = (Guid) _Session.Save(new Thing(4.6));
		}

		[vNextTransaction.Transaction]
		public virtual Thing VerifyThing()
		{
			Assert.That(_StatelessSession.Get<Thing>(_ThingId), Is.Not.Null);
			Assert.That(_StatelessSession.Transaction, Is.Not.Null);
			Assert.That(_Session.Transaction, Is.Not.Null);

			// for testing we need to make sure it's not just in the FLC.
			_Session.Clear();
			return _Session.Load<Thing>(_ThingId);
		}
	}
}