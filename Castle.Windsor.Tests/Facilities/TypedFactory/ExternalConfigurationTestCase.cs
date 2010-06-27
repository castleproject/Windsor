// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if (!SILVERLIGHT)

namespace Castle.Windsor.Tests.Facilities.TypedFactory
{
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using NUnit.Framework;

	[TestFixture]
	public class ExternalConfigurationTestCase
	{
		[SetUp]
		public void Init()
		{
			var path = ConfigHelper.ResolveConfigPath("Facilities/TypedFactory/typedFactory_castle_config.xml");

			container = new WindsorContainer();
			container.Install(Configuration.FromXmlFile(path));
			container.AddFacility<TypedFactoryFacility>("typedfactory");

			container.Register(
				Component.For(typeof(IProtocolHandler)).ImplementedBy(typeof(MirandaProtocolHandler)).Named("miranda"));
			container.Register(
				Component.For(typeof(IProtocolHandler)).ImplementedBy(typeof(MessengerProtocolHandler)).Named("messenger"));
			container.Register(Component.For(typeof(IDummyComponent)).ImplementedBy(typeof(Component1)).Named("comp1"));
			container.Register(Component.For(typeof(IDummyComponent)).ImplementedBy(typeof(Component2)).Named("comp2"));
		}

		[TearDown]
		public void Finish()
		{
			container.Dispose();
		}

		private IWindsorContainer container;

		[Test]
		public void Factory1()
		{
			var factory = (IProtocolHandlerFactory1)container["protocolFac1"];

			Assert.IsNotNull(factory);

			var handler = factory.Create();

			Assert.IsNotNull(handler);

			factory.Release(handler);
		}

		[Test]
		public void Factory2()
		{
			var factory = (IProtocolHandlerFactory2)container["protocolFac2"];

			Assert.IsNotNull(factory);

			var handler = factory.Create("miranda");
			Assert.IsNotNull(handler);
			Assert.IsTrue(handler is MirandaProtocolHandler);
			factory.Release(handler);

			handler = factory.Create("messenger");
			Assert.IsNotNull(handler);
			Assert.IsTrue(handler is MessengerProtocolHandler);
			factory.Release(handler);
		}

		[Test]
		public void Factory3()
		{
			var factory = (IComponentFactory1)container["compFactory1"];

			Assert.IsNotNull(factory);

			var comp1 = factory.Construct();
			Assert.IsNotNull(comp1);

			var comp2 = factory.Construct();
			Assert.IsNotNull(comp2);
		}

		[Test]
		public void Factory4()
		{
			var factory = (IComponentFactory2)container["compFactory2"];
			Assert.IsNotNull(factory);

			var comp1 = (IDummyComponent)factory.Construct("comp1");
			Assert.IsTrue(comp1 is Component1);
			Assert.IsNotNull(comp1);

			var comp2 = (IDummyComponent)factory.Construct("comp2");
			Assert.IsTrue(comp2 is Component2);
			Assert.IsNotNull(comp2);
		}

		[Test]
		public void No_Creation_Or_Destruction_methods_defined()
		{
			var factory = (IComponentFactory1)container["NoCreationOrDestructionDefined"];

			Assert.IsNotNull(factory);

			var comp1 = factory.Construct();
			Assert.IsNotNull(comp1);

			var comp2 = factory.Construct();
			Assert.IsNotNull(comp2);
		}

		[Test]
		public void Selector_in_xml()
		{
			container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>(),
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("one"));
			var factory = container.Resolve<IComponentFactory1>("HasOneSelector");
			var dummyComponent = factory.Construct();
			Assert.IsInstanceOf<Component2>(dummyComponent);
		}
	}
}

#endif