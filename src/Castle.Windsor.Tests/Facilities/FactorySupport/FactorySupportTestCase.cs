// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Facilities.FactorySupport
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Lifestyle.Components;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class FactorySupportTestCase : AbstractContainerTestCase
	{

		protected override void AfterContainerCreated()
		{
			Kernel.AddFacility<FactorySupportFacility>();
		}

		[Test]
		public void NullModelConfigurationBug()
		{
			Kernel.Register(Component.For<ICustomer>().Named("a").Instance(new CustomerImpl()));
		}

		[Test]
		public void DependencyIgnored()
		{
			Kernel.Register(Component.For(typeof(Factory)).Named("a"));

			AddComponent("stringdictComponent", typeof(StringDictionaryDependentComponent), "CreateWithStringDictionary");
			AddComponent("hashtableComponent", typeof(HashTableDependentComponent), "CreateWithHashtable");
			AddComponent("serviceComponent", typeof(ServiceDependentComponent), "CreateWithService");

			Kernel.Resolve("hashtableComponent", typeof(HashTableDependentComponent));
			Kernel.Resolve("serviceComponent", typeof(ServiceDependentComponent));
			Kernel.Resolve("stringdictComponent", typeof(StringDictionaryDependentComponent));
		}

#if !SILVERLIGHT
		[Test]
		[Ignore("BUG: not working")]
		public void Can_instantiate_abstract_service_via_factory()
		{
			Container.Install(Configuration.FromXmlFile(
				ConfigHelper.ResolveConfigPath("Configuration2/abstract_component_factory.xml")));

			Container.Resolve<IComponent>("abstract");
		}
#endif

		[Test]
		[Ignore("Bug confirmed, but cant fix it without undesired side effects")]
		public void KernelDoesNotTryToWireComponentsPropertiesWithFactoryConfiguration()
		{
			Kernel.Register(Component.For(typeof(Factory)).Named("a"));

			var model = AddComponent("cool.service", typeof(MyCoolServiceWithProperties), "CreateCoolService");

			model.Parameters.Add("someProperty", "Abc");

			var service = Kernel.Resolve<MyCoolServiceWithProperties>("cool.service");

			Assert.IsNotNull(service);
			Assert.IsNull(service.SomeProperty);
		}

		[Test]
		[Ignore("Since the facility is mostly for legacy stuff, I don't think it's crucial to support this.")]
		public void Late_bound_factory_properly_applies_lifetime_concerns()
		{
			Kernel.AddFacility<FactorySupportFacility>();
			Kernel.Register(Component.For(typeof(DisposableComponentFactory)).Named("a"));
			var componentModel = AddComponent("foo", typeof(IComponent), "Create");
			componentModel.LifestyleType = LifestyleType.Transient;
			var component = Kernel.Resolve<IComponent>("foo") as ComponentWithDispose;
			Assert.IsNotNull(component);
			Assert.IsFalse(component.Disposed);
			Kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Disposed);
		}

		private ComponentModel AddComponent(string key, Type type, string factoryMethod)
		{
			var config = new MutableConfiguration(key);
			config.Attributes["factoryId"] = "a";
			config.Attributes["factoryCreate"] = factoryMethod;
			Kernel.ConfigurationStore.AddComponentConfiguration(key, config);
			Kernel.Register(Component.For(type).Named(key));
			return Kernel.GetHandler(key).ComponentModel;
		}

		public class Factory
		{
			public static MyCoolServiceWithProperties CreateCoolService(string someProperty)
			{
				return new MyCoolServiceWithProperties();
			}

			public static HashTableDependentComponent CreateWithHashtable()
			{
				return new HashTableDependentComponent(null);
			}

			public static ServiceDependentComponent CreateWithService()
			{
				return new ServiceDependentComponent(null);
			}

			public static StringDictionaryDependentComponent CreateWithStringDictionary()
			{
				return new StringDictionaryDependentComponent(null);
			}
		}

		public class MyCoolServiceWithProperties
		{
			public string SomeProperty { get; set; }
		}

		public class StringDictionaryDependentComponent
		{
			public StringDictionaryDependentComponent(Dictionary<string, object> d)
			{
			}
		}

		public class ServiceDependentComponent
		{
			public ServiceDependentComponent(ICommon d)
			{
			}
		}

		public class HashTableDependentComponent
		{
			public HashTableDependentComponent(Dictionary<object, object> d)
			{
			}
		}
	}
}