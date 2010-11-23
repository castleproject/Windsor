namespace Castle.Facilities.NHibernateIntegration.Tests
{
	using Core.Resource;
	using MicroKernel.Facilities;
	using NUnit.Framework;
	using Windsor;
	using Windsor.Configuration.Interpreters;

	[TestFixture]
	public class FacilityFluentConfigTestCase
	{
		[Test]
		public void Should_override_DefaultConfigurationBuilder()
		{
			var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

			var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

			container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.CustomConfigurationBuilder = typeof(TestConfigurationBuilder));

			Assert.AreEqual(typeof(TestConfigurationBuilder), container.Resolve<IConfigurationBuilder>().GetType());
		}

		[Test, ExpectedException(typeof(FacilityException))]
		public void Should_not_accept_non_implementors_of_IConfigurationBuilder_for_override()
		{
			var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

			var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

			container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.CustomConfigurationBuilder = GetType());
		}
	}
}
