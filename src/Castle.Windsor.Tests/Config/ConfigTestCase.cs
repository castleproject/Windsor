namespace Castle.Windsor.Tests.Config
{
#if !SILVERLIGHT
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.Components;

	using NUnit.Framework;
	using Config = Castle.Windsor.Installer.Configuration;

	[TestFixture]
	public class ConfigTestCase
	{
		private IWindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
		}

		[Test]
		[Ignore("Not supported. Would be good to have, not sure if in this form or another")]
		public void Can_split_configuration_between_multiple_component_elements()
		{
			// seee http://stackoverflow.com/questions/3253975/castle-windsor-with-xml-includes-customization-problem for real life scenario
			container.Install(Config.FromXmlFile(ConfigHelper.ResolveConfigPath("Configuration/OneComponentInTwoPieces.xml")));
			var service = container.Resolve<ISimpleService>("Foo");
			var interceptor = container.Resolve<CountingInterceptor>("a");

			service.Operation();

			Assert.AreEqual(1, interceptor.InterceptedCallsCount);
		}
	}
#endif
}