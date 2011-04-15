using System;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	[Ignore("Retry policies are nicer to test with e.g. NHibernate integration parts.")]
	public class RetryPolicies_Transactions
	{
		private WindsorContainer _Container;
		
		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		// something like: 
		// http://philbolduc.blogspot.com/2010/03/retryable-actions-in-c.html

		[Test]
		public void retrying_twice_on_timeout()
		{
			// on app-start
			var txManager = _Container.Resolve<ITxManager>();
			var counter = 0;
			txManager.AddRetryPolicy("timeouts", e => e is TimeoutException && ++counter <= 2);

			// in controller's c'tor
			using (var s = new ResolveScope<IMyService>(_Container))
			{
				// in action
				s.Service.VerifyInAmbient(() =>
				{
					if (txManager.CurrentTransaction
						.Do(x => x.FailedPolicy)
						.Do(x => x.Failures < 2)
						.OrThrow(() => new Exception("Test failure; maybe doesn't have value!")))
						throw new TimeoutException("database not responding in a timely manner");
				});
			}
		}
	}
}