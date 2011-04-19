using System.Threading;
using Castle.Facilities.AutoTx.Testing;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net.Config;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class MultipleThreads_TransactionBookKeeping
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void TheCountInTheAmbientContextIsOne()
		{
			using (var scope = _Container.ResolveScope<IMyService>())
			{
				Assert.That(scope.Manager.Count, Is.EqualTo(0));
				scope.Service.VerifyInAmbient(() =>
				{
					Assert.That(scope.Manager.Count, Is.EqualTo(1));
					
					var mre = new ManualResetEvent(false);
					scope.Service.VerifyBookKeepingInFork(() =>
					{
						mre.WaitOne();
						Assert.That(scope.Manager.Count, Is.EqualTo(1), "because we're in a different call-context");
					});

					Assert.That(scope.Manager.Count, Is.EqualTo(1), "because we're in the same call context, and the dependent transaction is not our 'current', so the count needs to match");
					mre.Set();
				});
			}
		}
	}
}