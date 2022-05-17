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

namespace CastleTests
{
	using System;
	using System.Collections.Generic;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Proxy;
	using Castle.Windsor.Tests.MicroKernel;
	using CastleTests.Components;
	using NUnit.Framework;

	[TestFixture]
	public class MicroKernelTestCase : AbstractContainerTestCase
	{
		[Test]
		[Bug("IOC-327")]
		public void ReleaseComponent_null_silently_ignored_doesnt_throw()
		{
			Assert.DoesNotThrow(() => Kernel.ReleaseComponent(null));
		}

		[Test]
		public void AddClassComponentWithInterface()
		{
			Kernel.Register(Component.For<CustomerImpl>().Named("key"));
			Assert.IsTrue(Kernel.HasComponent("key"));
		}

		[Test]
		public void AddClassComponentWithNoInterface()
		{
			Kernel.Register(Component.For(typeof(DefaultCustomer)).Named("key"));
			Assert.IsTrue(Kernel.HasComponent("key"));
		}

		[Test]
		public void AddClassThatHasTwoParametersOfSameTypeAndNoOverloads()
		{
			Kernel.Register(Component.For(typeof(ClassWithTwoParametersWithSameType)).Named("test"));
			Kernel.Register(Component.For<ICommon>().ImplementedBy(typeof(CommonImpl1)).Named("test2"));
			var resolved = Kernel.Resolve(typeof(ClassWithTwoParametersWithSameType));
			Assert.IsNotNull(resolved);
		}

		[Test]
		public void AddCommonComponent()
		{
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("key"));
			Assert.IsTrue(Kernel.HasComponent("key"));
		}

		[Test]
		public void AddComponentInstance()
		{
			var customer = new CustomerImpl();
			Kernel.Register(Component.For<ICustomer>().Named("key").Instance(customer));
			Assert.IsTrue(Kernel.HasComponent("key"));

			var customer2 = Kernel.Resolve<CustomerImpl>("key");
			Assert.AreSame(customer, customer2);

			customer2 = Kernel.Resolve<ICustomer>() as CustomerImpl;
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddComponentInstance2()
		{
			var customer = new CustomerImpl();

			Kernel.Register(Component.For<CustomerImpl>().Named("key").Instance(customer));
			Assert.IsTrue(Kernel.HasComponent("key"));

			var customer2 = Kernel.Resolve<CustomerImpl>("key");
			Assert.AreSame(customer, customer2);

			customer2 = Kernel.Resolve<CustomerImpl>();
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddComponentInstance_ByService()
		{
			var customer = new CustomerImpl();

			Kernel.Register(Component.For<ICustomer>().Instance(customer));
			Assert.AreSame(Kernel.Resolve<ICustomer>(), customer);
		}

		[Test]
		public void AdditionalParametersShouldNotBePropagatedInTheDependencyChain()
		{
			Kernel.Register(
				Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("cust").LifeStyle.Transient);
			Kernel.Register(Component.For<ExtendedCustomer>().Named("custex").LifeStyle.Transient);

			var dictionary = new Arguments { { "Name", "name" }, { "Address", "address" }, { "Age", "18" } };
			var customer = Kernel.Resolve<ICustomer>("cust", dictionary);

			Assert.AreEqual("name", customer.Name);
			Assert.AreEqual("address", customer.Address);
			Assert.AreEqual(18, customer.Age);

			var custImpl = customer as CustomerImpl;

			Assert.IsNotNull(custImpl.ExtendedCustomer);
			Assert.IsNull(custImpl.ExtendedCustomer.Address);
			Assert.IsNull(custImpl.ExtendedCustomer.Name);
			Assert.AreEqual(0, custImpl.ExtendedCustomer.Age);
		}

		[Test]
		public void Can_use_custom_dependencyResolver()
		{
			var resolver = new NotImplementedDependencyResolver();
			var defaultKernel = new DefaultKernel(resolver, new DefaultProxyFactory());
			Assert.AreSame(resolver, defaultKernel.Resolver);
			Assert.AreSame(defaultKernel, resolver.Kernel);
		}

		[Test]
		public void HandlerForClassComponent()
		{
			Kernel.Register(Component.For<CustomerImpl>().Named("key"));
			var handler = Kernel.GetHandler("key");
			Assert.IsNotNull(handler);
		}

		[Test]
		public void HandlerForClassWithNoInterface()
		{
			Kernel.Register(Component.For<DefaultCustomer>().Named("key"));
			var handler = Kernel.GetHandler("key");
			Assert.IsNotNull(handler);
		}

		[Test]
		public void IOC_50_AddTwoComponentWithSameService_RequestFirstByKey_RemoveFirst_RequestByService_ShouldReturnSecond()
		{
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("key"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("key2"));
			var result = Kernel.Resolve<object>("key");
			Assert.IsNotNull(result);

			result = Kernel.Resolve<ICustomer>();
			Assert.IsNotNull(result);
		}

		[Test]
		public void KeyCollision()
		{
			Kernel.Register(Component.For<CustomerImpl>().Named("key"));
			Assert.Throws<ComponentRegistrationException>(() => Kernel.Register(Component.For<CustomerImpl>().Named("key")));
		}

		[Test]
		public void ResolveAll()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl2>());
			Kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>());
			var services = Kernel.ResolveAll<ICommon>();
			Assert.AreEqual(2, services.Length);
		}

		[Test]
		public void ResolveAll_does_NOT_account_for_assignable_services()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("test"));
			Kernel.Register(Component.For<ICommonSub1>().ImplementedBy<CommonSub1Impl>().Named("test2"));
			var services = Kernel.ResolveAll<ICommon>();
			Assert.AreEqual(1, services.Length);
		}

		[Test]
		public void ResolveAll_does_handle_multi_service_components()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("test"));
			Kernel.Register(Component.For<ICommonSub1, ICommon>().ImplementedBy<CommonSub1Impl>().Named("test2"));
			var services = Kernel.ResolveAll<ICommon>();
			Assert.AreEqual(2, services.Length);
		}

		[Test]
		public void ResolveAll_resolves_when_dependency_provideded_dynamically()
		{
			Kernel.Register(Component.For<ICommon>()
			                	.ImplementedBy<CommonImplWithDependency>()
			                	.DynamicParameters((k, d) => d.AddTyped(typeof(ICustomer), new CustomerImpl()))
				);

			var services = Kernel.ResolveAll<ICommon>();
			Assert.AreEqual(1, services.Length);
		}

		[Test]
		public void ResolveAll_resolves_when_dependency_provideded_inline()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy(typeof(CommonImplWithDependency)).Named("test"));
			var services = Kernel.ResolveAll<ICommon>(new Arguments().AddNamed("customer", new CustomerImpl()));
			Assert.AreEqual(1, services.Length);
		}

		[Test]
		public void ResolveUsingAddionalParametersForConfigurationInsteadOfServices()
		{
			Kernel.Register(
				Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("cust").LifeStyle.Is(
					LifestyleType.Transient));

			var customer = Kernel.Resolve<ICustomer>("cust");
			Assert.IsNull(customer.Address);
			Assert.IsNull(customer.Name);
			Assert.AreEqual(0, customer.Age);

			var dictionary = new Arguments { { "Name", "name" }, { "Address", "address" }, { "Age", "18" } };
			customer = Kernel.Resolve<ICustomer>("cust", dictionary);

			Assert.AreEqual("name", customer.Name);
			Assert.AreEqual("address", customer.Address);
			Assert.AreEqual(18, customer.Age);
		}

		[Test]
		public void ResolveViaGenerics()
		{
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("cust"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl2>().Named("cust2"));
			var customer = Kernel.Resolve<ICustomer>("cust");

			var dictionary = new Arguments {
				{ "name", "customer2Name" },
				{ "address", "customer2Address" },
				{ "age", 18 }
			};
			var customer2 = Kernel.Resolve<ICustomer>("cust2", dictionary);

			Assert.AreEqual(customer.GetType(), typeof(CustomerImpl));
			Assert.AreEqual(customer2.GetType(), typeof(CustomerImpl2));
		}

		[Test]
		public void Resolve_all_when_dependency_is_missing_throws_DependencyResolverException()
		{
			Kernel.Register(
				Component.For<C>());
			// the dependency goes C --> B --> A

			Assert.Throws<DependencyResolverException>(() => Kernel.ResolveAll<C>(new Arguments { { "fakeDependency", "Stefan!" } }));
		}

		[Test]
		public void Resolve_all_when_dependency_is_unresolvable_throws_HandlerException()
		{
			Kernel.Register(
				Component.For<B>(),
				Component.For<C>());
			// the dependency goes C --> B --> A

			Assert.Throws<HandlerException>(() => Kernel.ResolveAll<C>());
		}

		[Test]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_LifestyleType_And_Override_Signature()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy<BaseCommonComponent>().Named("abstract").LifeStyle.Pooled);

			var expectedMessage =
				string.Format(
					"Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instantiate it as implementation of service 'abstract'. Did you forget to proxy it?",
					Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				                                                      Kernel.Resolve<ICommon>("abstract"));
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_LifestyleType_Signature()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy<BaseCommonComponent>().Named("abstract").LifeStyle.Pooled);
			var expectedMessage =
				string.Format(
					"Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instantiate it as implementation of service 'abstract'. Did you forget to proxy it?",
					Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () => Kernel.Resolve<ICommon>("abstract"));
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_Simple_Signature()
		{
			Kernel.Register(Component.For<ICommon>().ImplementedBy<BaseCommonComponent>().Named("abstract"));

			var expectedMessage =
				string.Format(
					"Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instantiate it as implementation of service 'abstract'. Did you forget to proxy it?",
					Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () => Kernel.Resolve<ICommon>("abstract"));
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClass_With_LifestyleType_And_Override_Signature()
		{
			Kernel.Register(Component.For<BaseCommonComponent>().Named("abstract").LifeStyle.Pooled);

			var expectedMessage =
				string.Format(
					"Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instantiate it as implementation of service 'abstract'. Did you forget to proxy it?",
					Environment.NewLine);
			var exception =
				Assert.Throws<ComponentRegistrationException>(() => Kernel.Resolve<ICommon>("abstract"));
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClass_With_LifestyleType_Signature()
		{
			Kernel.Register(Component.For<BaseCommonComponent>().Named("abstract").LifeStyle.Pooled);

			var expectedMessage =
				string.Format(
					"Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instantiate it as implementation of service 'abstract'. Did you forget to proxy it?",
					Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () => Kernel.Resolve<ICommon>("abstract"));
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClass_With_Simple_Signature()
		{
			Kernel.Register(Component.For<BaseCommonComponent>().Named("abstract"));
			var expectedMessage =
				string.Format(
					"Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instantiate it as implementation of service 'abstract'. Did you forget to proxy it?",
					Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () => Kernel.Resolve<ICommon>("abstract"));
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void Subsystems_are_case_insensitive()
		{
			Assert.IsNotNull(Kernel.GetSubSystem(SubSystemConstants.ConfigurationStoreKey));
			Assert.IsNotNull(Kernel.GetSubSystem(SubSystemConstants.ConfigurationStoreKey.ToLower()));
			Assert.IsNotNull(Kernel.GetSubSystem(SubSystemConstants.ConfigurationStoreKey.ToUpper()));
		}

		[Test]
		public void UnregisteredComponentByKey()
		{
			Kernel.Register(Component.For<CustomerImpl>().Named("key1"));
			Assert.Throws<ComponentNotFoundException>(() => Kernel.Resolve<object>("key2"));
		}

		[Test]
		public void UnregisteredComponentByService()
		{
			Kernel.Register(Component.For<CustomerImpl>().Named("key1"));
			Assert.Throws<ComponentNotFoundException>(() => Kernel.Resolve<IDisposable>());
		}
	}
}