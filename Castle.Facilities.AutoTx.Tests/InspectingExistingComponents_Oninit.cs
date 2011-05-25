using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using Castle.Facilities.AutoTx.Testing;

namespace Castle.Facilities.AutoTx.Tests
{
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