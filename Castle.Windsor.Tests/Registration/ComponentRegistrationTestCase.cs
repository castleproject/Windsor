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

namespace Castle.MicroKernel.Tests.Registration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.DynamicProxy;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Configuration.Components;
	using Castle.MicroKernel.Tests.Lifestyle.Components;
	using Castle.Windsor.Tests.Facilities.Startable.Components;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	public class ComponentRegistrationTestCase : RegistrationTestCaseBase
	{
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddComponent_WhichIsNull_ThrowsNullArgumentException()
		{
			// Previously the kernel assummed everything was OK, and null reffed instead.
			Kernel.Register(Component.For(Type.GetType("NonExistentType, WohooAssembly")));
		}
		[Test]
		public void AddComponent_WithServiceOnly_RegisteredWithServiceTypeName()
		{
			Kernel.Register(
				Component.For<CustomerImpl>());

			var handler = Kernel.GetHandler(typeof(CustomerImpl));
			Assert.AreEqual(typeof(CustomerImpl), handler.Services.Single());
			Assert.AreEqual(typeof(CustomerImpl), handler.ComponentModel.Implementation);

			var customer = Kernel.Resolve<CustomerImpl>();
			Assert.IsNotNull(customer);

			var customer1 = Kernel.Resolve(typeof(CustomerImpl).FullName, new Arguments());
			Assert.IsNotNull(customer1);
			Assert.AreSame(customer, customer1);
		}

		[Test]
		public void AddComponent_WithInterceptorSelector_ComponentModelShouldHaveInterceptorSelector()
		{
			var selector = new InterceptorTypeSelector(typeof(TestInterceptor1));
			Kernel.Register(Component.For<ICustomer>().Interceptors(new InterceptorReference(typeof(TestInterceptor1)))
			                	.SelectedWith(selector).Anywhere);

			var handler = Kernel.GetHandler(typeof(ICustomer));

			var proxyOptions = ProxyUtil.ObtainProxyOptions(handler.ComponentModel, false);

			Assert.IsNotNull(proxyOptions);
			Assert.AreEqual(selector, proxyOptions.Selector.Resolve(null, null));
		}

		[Test]
		public void AddComponent_WithInterfaceServiceOnly_And_Interceptors_ProxyOptionsShouldNotHaveATarget()
		{
			Kernel.Register(
				Component.For<ICustomer>().Interceptors(new InterceptorReference(typeof(StandardInterceptor))).Anywhere);

			var handler = Kernel.GetHandler(typeof(ICustomer));

			var proxyOptions = ProxyUtil.ObtainProxyOptions(handler.ComponentModel, false);

			Assert.IsNotNull(proxyOptions);
			Assert.IsTrue(proxyOptions.OmitTarget);
		}

		[Test]
		public void AddComponent_WithServiceAndName_RegisteredNamed()
		{
			Kernel.Register(
				Component.For<CustomerImpl>()
					.Named("customer")
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual("customer", handler.ComponentModel.Name);
			Assert.AreEqual(typeof(CustomerImpl), handler.Services.Single());
			Assert.AreEqual(typeof(CustomerImpl), handler.ComponentModel.Implementation);

			var customer = Kernel.Resolve<CustomerImpl>("customer");
			Assert.IsNotNull(customer);
		}

		[Test]
		[ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "This component has already been assigned name 'customer'")]
		public void AddComponent_NamedAlreadyAssigned_ThrowsException()
		{
			Kernel.Register(
				Component.For<CustomerImpl>()
					.Named("customer")
					.Named("customer1")
				);
		}

		[Test]
		[ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "There is a component already registered for the given key customer")]
		public void AddComponent_WithSameName_ThrowsException()
		{
			Kernel.Register(
				Component.For<CustomerImpl>()
					.Named("customer"),
				Component.For<CustomerImpl>()
					.Named("customer")
				);
		}

		[Test]
		public void AddComponent_WithServiceAndClass_RegisteredWithClassTypeName()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>());

			var customer = Kernel.Resolve<ICustomer>();
			Assert.IsNotNull(customer);

			var customer1 = Kernel.Resolve(typeof(CustomerImpl).FullName, new Arguments());
			Assert.IsNotNull(customer1);
		}

		[Test]
		[ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage =
				"This component has already been assigned implementation Castle.MicroKernel.Tests.ClassComponents.CustomerImpl")]
		public void AddComponent_WithImplementationAlreadyAssigned_ThrowsException()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.ImplementedBy<CustomerImpl2>()
				);
		}

		[Test]
		public void AddComponent_Instance_UsesInstance()
		{
			var customer = new CustomerImpl();

			Kernel.Register(
				Component.For<ICustomer>()
					.Named("key")
					.Instance(customer)
				);
			Assert.IsTrue(Kernel.HasComponent("key"));
			var handler = Kernel.GetHandler("key");
			Assert.AreEqual(customer.GetType(), handler.ComponentModel.Implementation);

			var customer2 = Kernel.Resolve("key", new Arguments()) as CustomerImpl;
			Assert.AreSame(customer, customer2);

			customer2 = Kernel.Resolve<ICustomer>() as CustomerImpl;
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddComponent_Instance_UsesInstanceWithParameters()
		{
			var customer = new CustomerImpl2("ernst", "delft", 29);

			Kernel.Register(
				Component.For<ICustomer>()
					.Named("key")
					.Instance(customer)
				);
			Assert.IsTrue(Kernel.HasComponent("key"));
			var handler = Kernel.GetHandler("key");
			Assert.AreEqual(customer.GetType(), handler.ComponentModel.Implementation);

			var customer2 = Kernel.Resolve("key", new Arguments()) as CustomerImpl2;
			Assert.AreSame(customer, customer2);

			customer2 = Kernel.Resolve<ICustomer>() as CustomerImpl2;
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddComponent_WithExplicitLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.Is(LifestyleType.Transient)
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void AddComponent_WithTransientLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.Transient
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void AddComponent_WithSingletonLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.Singleton
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Singleton, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void AddComponent_WithCustomLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.Custom<CustomLifestyleManager>()
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void AddComponent_WithThreadLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.PerThread
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Thread, handler.ComponentModel.LifestyleType);
		}

#if (!SILVERLIGHT)
		[Test]
		public void AddComponent_WithPerWebRequestLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.PerWebRequest
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.PerWebRequest, handler.ComponentModel.LifestyleType);
		}
#endif

		[Test]
		public void AddComponent_WithPooledLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.Pooled
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Pooled, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void AddComponent_WithPooledWithSizeLifestyle_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.LifeStyle.PooledWithSize(5, 10)
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(LifestyleType.Pooled, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void AddComponent_Activator_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.Named("customer")
					.ImplementedBy<CustomerImpl>()
					.Activator<MyCustomerActivator>()
				);

			var handler = Kernel.GetHandler("customer");
			Assert.AreEqual(typeof(MyCustomerActivator), handler.ComponentModel.CustomComponentActivator);

			var customer = Kernel.Resolve<ICustomer>();
			Assert.AreEqual("James Bond", customer.Name);
		}

		[Test]
		public void AddComponent_ExtendedProperties_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.ExtendedProperties(
						Property.ForKey("key1").Eq("value1"),
						Property.ForKey("key2").Eq("value2")
					)
				);

			var handler = Kernel.GetHandler(typeof(ICustomer));
			Assert.AreEqual("value1", handler.ComponentModel.ExtendedProperties["key1"]);
			Assert.AreEqual("value2", handler.ComponentModel.ExtendedProperties["key2"]);
		}

		[Test]
		public void AddComponent_ExtendedProperties_UsingAnonymousType()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.ExtendedProperties(
						Property.ForKey("key1").Eq("value1"),
						Property.ForKey("key2").Eq("value2")));

			var handler = Kernel.GetHandler(typeof(ICustomer));
			Assert.AreEqual("value1", handler.ComponentModel.ExtendedProperties["key1"]);
			Assert.AreEqual("value2", handler.ComponentModel.ExtendedProperties["key2"]);
		}

		[Test]
		public void AddComponent_CustomDependencies_WorksFine()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.DependsOn(
						Property.ForKey("Name").Eq("Caption Hook"),
						Property.ForKey("Address").Eq("Fairyland"),
						Property.ForKey("Age").Eq(45)
					)
				);

			var customer = Kernel.Resolve<ICustomer>();
			Assert.AreEqual(customer.Name, "Caption Hook");
			Assert.AreEqual(customer.Address, "Fairyland");
			Assert.AreEqual(customer.Age, 45);
		}

		[Test]
		public void AddComponent_CustomDependencies_UsingAnonymousType()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.DependsOn(Property.ForKey("Name").Eq("Caption Hook"),
					           Property.ForKey("Address").Eq("Fairyland"),
					           Property.ForKey("Age").Eq(45)));

			var customer = Kernel.Resolve<ICustomer>();
			Assert.AreEqual(customer.Name, "Caption Hook");
			Assert.AreEqual(customer.Address, "Fairyland");
			Assert.AreEqual(customer.Age, 45);
		}

		[Test]
		public void AddComponent_CustomDependenciesDictionary_WorksFine()
		{
			var customDependencies = new Dictionary<string, object>();
			customDependencies["Name"] = "Caption Hook";
			customDependencies["Address"] = "Fairyland";
			customDependencies["Age"] = 45;

			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.DependsOn(customDependencies)
				);

			var customer = Kernel.Resolve<ICustomer>();
			Assert.AreEqual(customer.Name, "Caption Hook");
			Assert.AreEqual(customer.Address, "Fairyland");
			Assert.AreEqual(customer.Age, 45);
		}
		
		[Test]
		public void AddComponent_ArrayConfigurationParameters_WorksFine()
		{
			var list = new MutableConfiguration("list");
			list.Attributes.Add("type", typeof(ICommon).AssemblyQualifiedName);
			list.Children.Add(new MutableConfiguration("item", "${common1}"));
			list.Children.Add(new MutableConfiguration("item", "${common2}"));

			Kernel.Register(
				Component.For<ICommon>()
					.Named("common1")
					.ImplementedBy<CommonImpl1>(),
				Component.For<ICommon>()
					.Named("common2")
					.ImplementedBy<CommonImpl2>(),
				Component.For<ClassWithArrayConstructor>()
					.Parameters(
						Parameter.ForKey("first").Eq("${common2}"),
						Parameter.ForKey("services").Eq(list)
					)
				);

			var common1 = Kernel.Resolve<ICommon>("common1");
			var common2 = Kernel.Resolve<ICommon>("common2");
			var component = Kernel.Resolve<ClassWithArrayConstructor>();
			Assert.AreSame(common2, component.First);
			Assert.AreEqual(2, component.Services.Length);
			Assert.AreSame(common1, component.Services[0]);
			Assert.AreSame(common2, component.Services[1]);
		}

		[Test]
		public void AddComponent_ListConfigurationParameters_WorksFine()
		{
			var list = new MutableConfiguration("list");
			list.Attributes.Add("type", typeof(ICommon).AssemblyQualifiedName);
			list.Children.Add(new MutableConfiguration("item", "${common1}"));
			list.Children.Add(new MutableConfiguration("item", "${common2}"));

			Kernel.Register(
				Component.For<ICommon>()
					.Named("common1")
					.ImplementedBy<CommonImpl1>(),
				Component.For<ICommon>()
					.Named("common2")
					.ImplementedBy<CommonImpl2>(),
				Component.For<ClassWithListConstructor>()
					.Parameters(
						Parameter.ForKey("services").Eq(list)
					)
				);

			var common1 = Kernel.Resolve<ICommon>("common1");
			var common2 = Kernel.Resolve<ICommon>("common2");
			var component = Kernel.Resolve<ClassWithListConstructor>();
			Assert.AreEqual(2, component.Services.Count);
			Assert.AreSame(common1, component.Services[0]);
			Assert.AreSame(common2, component.Services[1]);
		}

		[Test]
		public void AddComponent_WithComplexConfiguration_WorksFine()
		{
			Kernel.Register(
				Component.For<ClassWithComplexParameter>()
					.Configuration(
						Child.ForName("parameters").Eq(
							Attrib.ForName("notUsed").Eq(true),
							Child.ForName("complexparam").Eq(
								Child.ForName("complexparametertype").Eq(
									Child.ForName("mandatoryvalue").Eq("value1"),
									Child.ForName("optionalvalue").Eq("value2")
									)
								)
							)
					)
				);

			var component = Kernel.Resolve<ClassWithComplexParameter>();
			Assert.IsNotNull(component);
			Assert.IsNotNull(component.ComplexParam);
			Assert.AreEqual("value1", component.ComplexParam.MandatoryValue);
			Assert.AreEqual("value2", component.ComplexParam.OptionalValue);
		}

		[Test]
		public void CanUseExistingComponentModelWithComponentRegistration()
		{
			Kernel.Register(Component.For<ICustomer>()
			                	.ImplementedBy<CustomerImpl>()
				);

			var handler = Kernel.GetHandler(typeof(ICustomer));
			var component = Component.For(handler.ComponentModel);

			Assert.AreEqual(typeof(CustomerImpl), component.Implementation);
		}

		[Test]
		public void AddGenericComponent_WithParameters()
		{
			Kernel.Register(Component.For(typeof(IGenericClassWithParameter<>))
			                	.ImplementedBy(typeof(GenericClassWithParameter<>))
			                	.Parameters(Parameter.ForKey("name").Eq("NewName"))
				);

			var instance = Kernel.Resolve<IGenericClassWithParameter<int>>();
			Assert.AreEqual("NewName", instance.Name);
		}

		[Test]
		public void AddComponent_StartableWithInterface_StartsComponent()
		{
			Kernel.AddFacility<StartableFacility>()
				.Register(Component.For<StartableComponent>());

			var component = Kernel.Resolve<StartableComponent>();

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			Kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}

		[Test]
		public void AddComponent_StartableWithoutInterface_StartsComponent()
		{
			Kernel.AddFacility<StartableFacility>()
				.Register(Component.For<NoInterfaceStartableComponent>()
				          	.StartUsingMethod("Start")
				          	.StopUsingMethod("Stop")
				);

			var component = Kernel.Resolve<NoInterfaceStartableComponent>();

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			Kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}

		[Test]
		public void AddComponent_StartableWithoutInterface_StartsComponent_via_expression()
		{
			Kernel.AddFacility<StartableFacility>()
				.Register(Component.For<NoInterfaceStartableComponent>()
				          	.StartUsingMethod(x => x.Start)
				          	.StopUsingMethod(x => x.Stop)
				);

			var component = Kernel.Resolve<NoInterfaceStartableComponent>();

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			Kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}
	}
}