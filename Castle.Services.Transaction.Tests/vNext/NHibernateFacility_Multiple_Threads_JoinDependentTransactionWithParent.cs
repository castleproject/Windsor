using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Transactions;
using Castle.Facilities.NHibernate;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction;
using Castle.Windsor;
using log4net;
using log4net.Config;
using NHibernate;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class NHibernateFacility_Multiple_Threads_JoinDependentTransactionWithParent
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
			_Container = new WindsorContainer();
			_Container.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());
			_Container.AddFacility<NHibernateFacility>();
			_Container.Register(Component.For<ThreadedService>().LifeStyle.Transient);
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void SameSessionInSameTransaction()
		{
			using (var threaded = new ResolveScope<ThreadedService>(_Container))
				threaded.Service.VerifySameSession();
		}

		[Test]
		public void SameSession_WithRecursion()
		{
			using (var threaded = new ResolveScope<ThreadedService>(_Container))
				threaded.Service.VerifyRecursingSession();
		}

		[Test]
		public void WeCanFork_NewTransaction_Means_AnotherISessionReference()
		{
			using (var txM = new ResolveScope<ITxManager>(_Container))
			using (var threaded = new ResolveScope<ThreadedService>(_Container))
			{
				threaded.Service.MainThreadedEntry();
			}
		}
	}

	public class ThreadedService
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ThreadedService));

		private Guid _MainThing;
		private List<Guid> _NextThing = new List<Guid>();

		private Guid _FirstSessionId;
		private readonly Func<ISession> _GetSession;

		public ThreadedService(Func<ISession> getSession)
		{
			Contract.Requires(getSession != null);
			_GetSession = getSession;
		}

		[vNextTransaction.Transaction]
		public virtual void VerifySameSession()
		{
			var s = _GetSession();
			var id1 = s.GetSessionImplementation().SessionId;
			
			var s2 = _GetSession();
			Assert.That(s2.GetSessionImplementation().SessionId, Is.EqualTo(id1));
		}

		[vNextTransaction.Transaction]
		public virtual void VerifyRecursingSession()
		{
			var myId = _GetSession().GetSessionImplementation().SessionId;
			CheckRecursingSession(myId);
		}

		[vNextTransaction.Transaction]
		protected virtual void CheckRecursingSession(Guid myId)
		{
			Assert.That(myId, Is.EqualTo(_GetSession().GetSessionImplementation().SessionId));
		}

		[vNextTransaction.Transaction]
		public virtual void MainThreadedEntry()
		{
			var s = _GetSession();
			_FirstSessionId  = s.GetSessionImplementation().SessionId;

			_MainThing = (Guid)s.Save(new Thing(17.0));

			_Logger.DebugFormat("put some cores ({0}) to work!", Environment.ProcessorCount);

			for (int i = 0; i < Environment.ProcessorCount; i++)
				CalculatePi();
		}

		[vNextTransaction.Transaction(Fork = true)]
		protected virtual void CalculatePi()
		{
			var s = _GetSession();

			Assert.That(s.GetSessionImplementation().SessionId, Is.Not.EqualTo(_FirstSessionId), 
				"because ISession is not thread safe and we want per-transaction semantics when Fork=true");

			lock(_NextThing)
				_NextThing.Add((Guid) s.Save(new Thing(2*CalculatePiInner(1))));
		}

		protected double CalculatePiInner(int i)
		{
			if (i == 5000)
				return 1;

			return 1 + i / (2.0 * i + 1) * CalculatePiInner(i + 1);
		}
	}
}