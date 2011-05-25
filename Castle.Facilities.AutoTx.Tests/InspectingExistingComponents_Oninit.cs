namespace Castle.Facilities.Transactions.Tests
{
	using MicroKernel.Registration;
	using NUnit.Framework;
	using TestClasses;
	using Testing;
	using Windsor;

	public class InspectingExistingComponents_OnInit
	{
		[Test]
		public void Register_Then_AddFacility_ThenInvokeTransactionalMethod()
		{
			var container = new WindsorContainer()
				.Register(Component.For<MyService2>().LifeStyle.Transient)
				.AddFacility<AutoTxFacility>();

			// this throws if we have not implemented this feature
			using (var s = container.ResolveScope<MyService2>())
				s.Service.VerifyInAmbient();
		}
	}
}