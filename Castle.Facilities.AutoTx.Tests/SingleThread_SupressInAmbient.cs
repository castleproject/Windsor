namespace Castle.Facilities.Transactions.Tests
{
	using MicroKernel.Registration;
	using NUnit.Framework;
	using TestClasses;
	using Testing;
	using Windsor;

	public class SingleThread_SupressInAmbient
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<MyService2>());
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void SupressedTransaction_NoCurrentTransaction()
		{
			using (var scope = new ResolveScope<MyService2>(_Container))
				scope.Service.VerifySupressed();
		}
		[Test]
		public void SupressedTransaction_InCurrentTransaction()
		{
			using (var scope = new ResolveScope<MyService2>(_Container))
				scope.Service.VerifyInAmbient(() => scope.Service.VerifySupressed());
		}
	}
}