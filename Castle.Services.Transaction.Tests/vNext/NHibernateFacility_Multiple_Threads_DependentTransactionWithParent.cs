using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
	public class NHibernateFacility_Multiple_Threads_DependentTransactionWithParent
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
		public void Forking_NewTransaction_Means_AnotherISessionReference()
		{
			using (var threaded = new ResolveScope<ThreadedService>(_Container))
			{
				threaded.Service.MainThreadedEntry();
				Assert.That(threaded.Service.CalculationsIds.Count, Is.EqualTo(Environment.ProcessorCount));
			}
		}

		[Test]
		public void Forking_InDependentTransaction_Means_PerTransactionLifeStyle_SoSameInstances()
		{
			using (var threaded = new ResolveScope<ThreadedService>(_Container))
				threaded.Service.VerifySameSessionInFork();
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

		public List<Guid> CalculationsIds
		{
			get { return _CalculationsIds; }
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

		#region Forking - Succeeding transactions

		[vNextTransaction.Transaction]
		public virtual void MainThreadedEntry()
		{
			var s = _GetSession();

			_MainThing = (Guid)s.Save(new Thing(17.0));

			_Logger.DebugFormat("put some cores ({0}) to work!", Environment.ProcessorCount);

			for (int i = 0; i < Environment.ProcessorCount; i++)
				CalculatePi(s.GetSessionImplementation().SessionId);
		}

		[vNextTransaction.Transaction(Fork = true)]
		protected virtual void CalculatePi(Guid firstSessionId)
		{
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

		#region Forking: (PerTransaction = same sessions per tx)

		[vNextTransaction.Transaction]
		public virtual void VerifySameSessionInFork()
		{
			_Logger.Info("asserting for main thread");

			AssertSameSessionId();

			_Logger.Info("forking");
			VerifySameSessionInForkInner();
		}

		[vNextTransaction.Transaction(Fork = true)]
		protected virtual void VerifySameSessionInForkInner()
		{
			_Logger.Info("asserting for task-thread");
			AssertSameSessionId();
		}

		private void AssertSameSessionId()
		{
			var s1 = _GetSession().GetSessionImplementation().SessionId;
			var s2 = _GetSession().GetSessionImplementation().SessionId;

			if (!s1.Equals(s2))
				_Logger.Error("s1 != s2 in forked method");

			Assert.That(s1, Is.EqualTo(s2));
		}

		#endregion
	}
}