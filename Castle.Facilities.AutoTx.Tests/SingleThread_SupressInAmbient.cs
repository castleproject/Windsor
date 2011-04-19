using Castle.Facilities.AutoTx.Testing;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class SingleThread_SupressInAmbient
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

		[Test]
		public void SupressedTransaction_NoCurrentTransaction()
		{
			using (var scope = new ResolveScope<IMyService>(_Container))
				scope.Service.VerifySupressed();
		}
		[Test]
		public void SupressedTransaction_InCurrentTransaction()
		{
			using (var scope = new ResolveScope<IMyService>(_Container))
				scope.Service.VerifyInAmbient(() => scope.Service.VerifySupressed());
		}
	}
}