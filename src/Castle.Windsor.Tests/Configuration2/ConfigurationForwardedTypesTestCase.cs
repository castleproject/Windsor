namespace Castle.Windsor.Tests.Configuration2
{
	using System;
	using System.IO;

	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationForwardedTypesTestCase
	{

		[SetUp]
		public void SetUp()
		{

			var dir = ConfigHelper.ResolveConfigPath("Configuration2/");
			var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir + "config_with_forwarded_types.xml");
			container = new WindsorContainer(file);
		}

		private IWindsorContainer container;

		[Test]
		public void Component_with_forwarded_types()
		{
			var first = container.Resolve<ICommon>("hasForwards");
			var second = container.Resolve<ICommon2>();
			Assert.AreSame(first, second);
		}
	}
}