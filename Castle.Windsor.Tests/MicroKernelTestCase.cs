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

namespace Castle.MicroKernel.Tests
{
	using System;
	using System.Collections.Generic;
	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class MicroKernelTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void IOC_50_AddTwoComponentWithSameService_RequestFirstByKey_RemoveFirst_RequestByService_ShouldReturnSecond()
		{
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("key"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("key2"));
			object result = kernel.Resolve("key", new Arguments());
			Assert.IsNotNull(result);

			kernel.RemoveComponent("key");

			result = kernel.Resolve<ICustomer>();
			Assert.IsNotNull(result);
		}

		[Test]
		public void AddClassComponentWithInterface()
		{
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
			Assert.IsTrue(kernel.HasComponent("key"));
		}

		[Test]
		public void AddClassComponentWithNoInterface()
		{
			kernel.Register(Component.For(typeof(DefaultCustomer)).Named("key"));
			Assert.IsTrue(kernel.HasComponent("key"));
		}

		[Test]
		public void AddComponentInstance()
		{
			CustomerImpl customer = new CustomerImpl();
		    kernel.Register(Component.For<ICustomer>().Named("key").Instance(customer));
			Assert.IsTrue(kernel.HasComponent("key"));

			CustomerImpl customer2 = kernel.Resolve("key", new Arguments()) as CustomerImpl;
			Assert.AreSame(customer, customer2);

			customer2 = kernel.Resolve<ICustomer>() as CustomerImpl;
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddComponentInstance_ByService()
		{
			CustomerImpl customer = new CustomerImpl();

			kernel.Register(Component.For(typeof(ICustomer)).Instance(customer));
			Assert.AreSame(kernel.Resolve<ICustomer>(), customer);
		}

		[Test]
		public void AddComponentInstance2()
		{
			var customer = new CustomerImpl();

			kernel.Register(Component.For<CustomerImpl>().Named("key").Instance(customer));
			Assert.IsTrue(kernel.HasComponent("key"));

			var customer2 = kernel.Resolve<CustomerImpl>("key");
			Assert.AreSame(customer, customer2);

			customer2 = kernel.Resolve<CustomerImpl>();
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddCommonComponent()
		{
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("key"));
			Assert.IsTrue(kernel.HasComponent("key"));
		}

		[Test]
		public void HandlerForClassComponent()
		{
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
			IHandler handler = kernel.GetHandler("key");
			Assert.IsNotNull(handler);
		}

		[Test]
		public void HandlerForClassWithNoInterface()
		{
			kernel.Register(Component.For(typeof(DefaultCustomer)).Named("key"));
			IHandler handler = kernel.GetHandler("key");
			Assert.IsNotNull(handler);
		}

		[Test]
		[ExpectedException(typeof(ComponentRegistrationException))]
		public void KeyCollision()
		{
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
		}

		[Test]
		[ExpectedException(typeof(ComponentNotFoundException))]
		public void UnregisteredComponentByKey()
		{
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key1"));
			object component = kernel.Resolve("key2", new Arguments());
		}

		[Test]
		[ExpectedException(typeof(ComponentNotFoundException))]
		public void UnregisteredComponentByService()
		{
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key1"));
			kernel.Resolve<IDisposable>();
		}

		[Test]
		public void AddClassThatHasTwoParametersOfSameTypeAndNoOverloads()
		{
			kernel.Register(Component.For(typeof(ClassWithTwoParametersWithSameType)).Named("test"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("test2"));
			object resolved = kernel.Resolve(typeof(ClassWithTwoParametersWithSameType), new Dictionary<object,object>());
			Assert.IsNotNull(resolved);
		}

		[Test]
		public void ResolveAll()
		{
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("test"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("test2"));
			ICommon[] services = kernel.ResolveAll<ICommon>();
			Assert.AreEqual(2, services.Length);
		}

		[Test]
		public void ResolveAllAccountsForAssignableServices()
		{
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("test"));
			kernel.Register(Component.For(typeof(ICommonSub1)).ImplementedBy(typeof(CommonSub1Impl)).Named("test2"));
			ICommon[] services = kernel.ResolveAll<ICommon>();
			Assert.AreEqual(2, services.Length);
		}

		[Test]
		public void ResolveAllWaitingOnDependencies()
		{
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImplWithDependency)).Named("test"));
			ICommon[] services = kernel.ResolveAll<ICommon>();
			Assert.AreEqual(0, services.Length);
		}

		[Test]
		public void ResolveAll_resolves_when_dependency_provideded_inline()
		{
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImplWithDependency)).Named("test"));
			ICommon[] services = kernel.ResolveAll<ICommon>(new Arguments().Insert("customer", new CustomerImpl()));
			Assert.AreEqual(1, services.Length);
		}

		[Test]
		public void ResolveAll_resolves_when_dependency_provideded_dynamically()
		{
			kernel.Register(Component.For<ICommon>()
			                	.ImplementedBy<CommonImplWithDependency>()
			                	.DynamicParameters((k, d) => d.Insert<ICustomer>(new CustomerImpl()))
				);

			var services = kernel.ResolveAll<ICommon>();
			Assert.AreEqual(1, services.Length);
		}

		[Test]
		public void Resolve_all_when_dependency_is_unresolvable_should_not_throw()
		{
			kernel.Register(
				Component.For<B>(), 
				Component.For<C>());
			// the dependency goes C --> B --> A

			C[] cs = null;
			Assert.DoesNotThrow(() => 
				cs = kernel.ResolveAll<C>());
			Assert.IsEmpty(cs);
		}

		[Test]
		public void Resolve_all_when_dependency_is_missing_should_not_throw()
		{
			kernel.Register(
				Component.For<C>());
			// the dependency goes C --> B --> A

			C[] cs = null;
			Assert.DoesNotThrow(() =>
			                    cs = kernel.ResolveAll<C>(new Arguments().Insert("fakeDependency", "Stefan!")));
			Assert.IsEmpty(cs);
		}

		[Test]
		public void ResolveViaGenerics()
		{
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("cust"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl2)).Named("cust2"));
			var customer = kernel.Resolve<ICustomer>("cust");

			var dictionary = new Dictionary<string, object>
			{
			    { "name", "customer2Name" },
			    { "address", "customer2Address" },
			    { "age", 18 }
			};
		    var customer2 = kernel.Resolve<ICustomer>("cust2", dictionary);

			Assert.AreEqual(customer.GetType(), typeof(CustomerImpl));
			Assert.AreEqual(customer2.GetType(), typeof(CustomerImpl2));
		}

		[Test]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_Simple_Signature()
		{
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(BaseCommonComponent)).Named("abstract"));

			string expectedMessage = string.Format("Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instansiate it as implementation of Castle.MicroKernel.Tests.ClassComponents.ICommon service", Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				{
					kernel.Resolve<ICommon>("abstract");
				});
			Assert.AreEqual(expectedMessage,exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClass_With_Simple_Signature()
		{
			kernel.Register(Component.For(typeof(BaseCommonComponent)).Named("abstract"));
			string expectedMessage = string.Format("Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instansiate it as implementation of Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent service", Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				{
					kernel.Resolve<ICommon>("abstract");
				});
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_LifestyleType_Signature()
		{
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(BaseCommonComponent)).Named("abstract").LifeStyle.Is(LifestyleType.Pooled));
			string expectedMessage = string.Format("Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instansiate it as implementation of Castle.MicroKernel.Tests.ClassComponents.ICommon service", Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				{
					kernel.Resolve<ICommon>("abstract");
				});
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClass_With_LifestyleType_Signature()
		{
			kernel.Register(Component.For(typeof(BaseCommonComponent)).Named("abstract").LifeStyle.Is(LifestyleType.Pooled));

			string expectedMessage = string.Format("Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instansiate it as implementation of Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent service", Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				{
					kernel.Resolve<ICommon>("abstract");
				});
			Assert.AreEqual(expectedMessage, exception.Message);
		}


		[Test]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_LifestyleType_And_Override_Signature()
		{
            kernel.Register(Component.For<ICommon>().ImplementedBy<BaseCommonComponent>().Named("abstract").LifeStyle.Pooled);

			string expectedMessage = string.Format("Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instansiate it as implementation of Castle.MicroKernel.Tests.ClassComponents.ICommon service", Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				{
					kernel.Resolve<ICommon>("abstract");
				});
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ShouldNotRegisterAbstractClass_With_LifestyleType_And_Override_Signature()
		{
            kernel.Register(Component.For<BaseCommonComponent>().Named("abstract").LifeStyle.Pooled);

			string expectedMessage = string.Format("Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.{0} As such, it is not possible to instansiate it as implementation of Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent service", Environment.NewLine);
			var exception =
				Assert.Throws(typeof(ComponentRegistrationException), () =>
				{
					kernel.Resolve<ICommon>("abstract");
				});
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ResolveUsingAddionalParametersForConfigurationInsteadOfServices()
		{
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("cust").LifeStyle.Is(LifestyleType.Transient));

			var customer = kernel.Resolve<ICustomer>("cust");
			Assert.IsNull(customer.Address);
			Assert.IsNull(customer.Name);
			Assert.AreEqual(0, customer.Age);

			var dictionary = new Dictionary<string, object> { { "Name", "name" }, { "Address", "address" }, { "Age", "18" } };
		    customer = kernel.Resolve<ICustomer>("cust", dictionary);

			Assert.AreEqual("name", customer.Name);
			Assert.AreEqual("address", customer.Address);
			Assert.AreEqual(18, customer.Age);
		}

		[Test]
		public void AdditionalParametersShouldNotBePropagatedInTheDependencyChain()
		{
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("cust").LifeStyle.Is(LifestyleType.Transient));
			kernel.Register(Component.For(typeof(ExtendedCustomer)).Named("custex").LifeStyle.Is(LifestyleType.Transient));

			var dictionary = new Dictionary<string, object> { { "Name", "name" }, { "Address", "address" }, { "Age", "18" } };
		    var customer = kernel.Resolve<ICustomer>("cust", dictionary);

			Assert.AreEqual("name", customer.Name);
			Assert.AreEqual("address", customer.Address);
			Assert.AreEqual(18, customer.Age);

			var custImpl = customer as CustomerImpl;

			Assert.IsNotNull(custImpl.ExtendedCustomer);
			Assert.IsNull(custImpl.ExtendedCustomer.Address);
			Assert.IsNull(custImpl.ExtendedCustomer.Name);
			Assert.AreEqual(0, custImpl.ExtendedCustomer.Age);
		}
	}
}
