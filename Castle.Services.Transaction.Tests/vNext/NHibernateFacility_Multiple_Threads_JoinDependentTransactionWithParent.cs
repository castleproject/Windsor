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
			using (var threaded = new ResolveScope<ThreadedService>(_Container))
				threaded.Service.MainThreadedEntry();
		}
	}

	public class ThreadedService
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ThreadedService));

		private readonly List<Guid> _CalculationsIds = new List<Guid>();

		private readonly Func<ISession> _GetSession;
		private readonly ITxManager _Manager;
		private Guid _MainThing;

		public ThreadedService(Func<ISession> getSession, ITxManager manager)
		{
			Contract.Requires(manager != null);
			Contract.Requires(getSession != null);

			_GetSession = getSession;
			_Manager = manager;
		}

		#region Same instance tests

		[vNextTransaction.Transaction]
		public virtual void VerifySameSession()
		{
			var s = _GetSession();
			var id1 = s.GetSessionImplementation().SessionId;
			
			var s2 = _GetSession();
			Assert.That(s2.GetSessionImplementation().SessionId, Is.EqualTo(id1));
		}

		#endregion

		#region Recursion/multiple txs on call context

		[vNextTransaction.Transaction]
		public virtual void VerifyRecursingSession()
		{
			var myId = _GetSession().GetSessionImplementation().SessionId;
			CheckRecursingSession_ShouldBeDifferent(myId);
			CheckRecursingSessionWithoutTransaction_ShouldBeSame(myId);
		}

		[vNextTransaction.Transaction]
		protected virtual void CheckRecursingSession_ShouldBeDifferent(Guid myId)
		{
			var session = _GetSession();
			Assert.That(myId, Is.Not.EqualTo(session.GetSessionImplementation().SessionId));
		}

		protected virtual void CheckRecursingSessionWithoutTransaction_ShouldBeSame(Guid myId)
		{
			var session = _GetSession();
			Assert.That(myId, Is.EqualTo(session.GetSessionImplementation().SessionId));
		}

		#endregion

		#region Forked transaction tests

		[vNextTransaction.Transaction]
		public virtual void MainThreadedEntry()
		{
			var s = _GetSession();

			_MainThing = (Guid)s.Save(new Thing(17.0));

			_Logger.DebugFormat("put some cores ({0}) to work!", Environment.ProcessorCount);

			for (int i = 0; i < Environment.ProcessorCount * 10; i++)
				CalculatePi(s.GetSessionImplementation().SessionId);

			lock (_CalculationsIds)
				Assert.That(_CalculationsIds.Count, Is.EqualTo(Environment.ProcessorCount * 10));
		}

		[vNextTransaction.Transaction(Fork = true)]
		protected virtual void CalculatePi(Guid firstSessionId)
		{
			_Logger.DebugFormat("current no of transactions in context {0}", _Manager.Count);

			var s = _GetSession();

			Assert.That(s.GetSessionImplementation().SessionId, Is.Not.EqualTo(firstSessionId), 
			            "because ISession is not thread safe and we want per-transaction semantics when Fork=true");

			lock(_CalculationsIds)
				_CalculationsIds.Add((Guid) s.Save(new Thing(2*CalculatePiInner(1))));
		}

		protected double CalculatePiInner(int i)
		{
			if (i == 5000)
				return 1;

			return 1 + i / (2.0 * i + 1) * CalculatePiInner(i + 1);
		}

		#endregion
	}
}