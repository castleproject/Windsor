// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.Facilities.FactorySupport
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class FactorySupportTestCase
	{
		IKernel kernel;

		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
		}

		[Test]
		public void NullModelConfigurationBug()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.AddComponentInstance("a", new CustomerImpl());
		}

		[Test]
		public void DependancyIgnored()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.AddComponent("a", typeof(Factory));

			AddComponent("stringdictComponent", typeof(StringDictionaryDependentComponent), "CreateWithStringDictionary");
			AddComponent("hashtableComponent", typeof(HashTableDependentComponent), "CreateWithHashtable");
			AddComponent("serviceComponent", typeof(ServiceDependentComponent), "CreateWithService");

			kernel.Resolve("hashtableComponent", typeof(HashTableDependentComponent));
			kernel.Resolve("serviceComponent", typeof(ServiceDependentComponent));
			kernel.Resolve("stringdictComponent", typeof(StringDictionaryDependentComponent));
		}

		[Test, Ignore("Bug confirmed, but cant fix it without undesired side effects")]
		public void KernelDoesNotTryToWireComponentsPropertiesWithFactoryConfiguration()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.AddComponent("a", typeof(Factory));

			ComponentModel model = AddComponent("cool.service", typeof(MyCoolServiceWithProperties), "CreateCoolService");

			model.Parameters.Add("someProperty", "Abc");

			var service = (MyCoolServiceWithProperties)kernel["cool.service"];

			Assert.IsNotNull(service);
			Assert.IsNull(service.SomeProperty);
		}

		[Test]
		public void CheckReturnValueFromTheFactory()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.Register(
				Component.For<PetFactory>().Parameters(
					Parameter.ForKey("petType").Eq("dog"))
					.Named("pet.factory"));
			kernel.Register(
				Component.For<Pet>().ImplementedBy<Cat>()
					.Configuration(
					Attrib.ForName("factoryId").Eq("pet.factory"),
					Attrib.ForName("factoryCreate").Eq("Get"))
				);

			Assert.Throws(typeof(FacilityException), () => kernel.Resolve<Pet>());
		}

		[Test]
		public void CheckReturnValueFromTheFactoryAccessor()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.Register(
				Component.For<PetFactory>()
					.Named("pet.factory"));
			kernel.Register(
				Component.For<Pet>().ImplementedBy<Cat>()
					.Configuration(
					Attrib.ForName("instance-accessor").Eq("Dog"))
				);
			Assert.Throws(typeof(FacilityException), () => kernel.Resolve<Pet>());
		}

		[Test]
		public void Works_for_dependencies()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.Register(
				Component.For<Pet>().ImplementedBy<Cat>()
					.UsingFactoryMethod(() => new Cat()));
			kernel.Register(Component.For<UsesPet>());

			kernel.Resolve<UsesPet>();
		}

		private ComponentModel AddComponent(string key, Type type, string factoryMethod)
		{
			MutableConfiguration config = new MutableConfiguration(key);
			config.Attributes["factoryId"] = "a";
			config.Attributes["factoryCreate"] = factoryMethod;
			kernel.ConfigurationStore.AddComponentConfiguration(key, config);
			kernel.AddComponent(key, type);
			return kernel.GetHandler(key).ComponentModel;
		}

		public class Factory
		{
			public static HashTableDependentComponent CreateWithHashtable()
			{
				return new HashTableDependentComponent(null);
			}

			public static StringDictionaryDependentComponent CreateWithStringDictionary()
			{
				return new StringDictionaryDependentComponent(null);
			}

			public static ServiceDependentComponent CreateWithService()
			{
				return new ServiceDependentComponent(null);
			}

			public static MyCoolServiceWithProperties CreateCoolService(string someProperty)
			{
				return new MyCoolServiceWithProperties();
			}
		}

		public class MyCoolServiceWithProperties
		{
			private string someProperty;

			public string SomeProperty
			{
				get { return someProperty; }
				set { someProperty = value; }
			}
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

		public class Cat : Pet
		{
			public static Pet Dog
			{
				get { return new Dog(); }
			}
		}
		public class Dog : Pet
		{
		}

		public class Pet
		{
		}

		public class UsesPet
		{
			public Pet Pet { get; set; }

			public UsesPet(Pet pet)
			{
				Pet = pet;
			}
		}

		public class PetFactory
		{
			private readonly string type;
			public PetFactory(string petType)
			{
				type = petType;
			}
			public Pet Get()
			{
				if (type == "cat")
					return new Cat();
				else
					return new Dog();
			}
		}


	}
}
