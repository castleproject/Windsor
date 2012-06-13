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

namespace CastleTests.Activators
{
	using System;

	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Configuration.Components;
	using Castle.Windsor.Tests;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class BestConstructorTestCase : AbstractContainerTestCase
	{
		[Test]
		public void ConstructorWithMoreArguments()
		{
			Container.Register(Component.For<A>(),
			                   Component.For<B>(),
			                   Component.For<C>(),
			                   Component.For<ServiceUser>());

			var service = Container.Resolve<ServiceUser>();

			Assert.IsNotNull(service);
			Assert.IsNotNull(service.AComponent);
			Assert.IsNotNull(service.BComponent);
			Assert.IsNotNull(service.CComponent);
		}

		[Test]
		public void ConstructorWithOneArgument()
		{
			Container.Register(Component.For<A>().Named("a"),
			                   Component.For<ServiceUser>().Named("service"));

			var service = Container.Resolve<ServiceUser>("service");

			Assert.IsNotNull(service);
			Assert.IsNotNull(service.AComponent);
			Assert.IsNull(service.BComponent);
			Assert.IsNull(service.CComponent);
		}

		[Test]
		public void ConstructorWithTwoArguments()
		{
			Container.Register(Component.For<A>().Named("a"),
			                   Component.For<B>().Named("b"),
			                   Component.For<ServiceUser>().Named("service"));

			var service = Container.Resolve<ServiceUser>("service");

			Assert.IsNotNull(service);
			Assert.IsNotNull(service.AComponent);
			Assert.IsNotNull(service.BComponent);
			Assert.IsNull(service.CComponent);
		}

		[Test]
		public void DefaultComponentActivator_is_used_by_default()
		{
			Container.Register(Component.For<A>());

			var handler = Kernel.GetHandler(typeof(A));
			var activator = ((IKernelInternal)Kernel).CreateComponentActivator(handler.ComponentModel);

			Assert.IsInstanceOf<DefaultComponentActivator>(activator);
		}

		[Test]
		public void ParametersAndServicesBestCase()
		{
			var store = new DefaultConfigurationStore();

			var config = new MutableConfiguration("component");
			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);
			parameters.Children.Add(new MutableConfiguration("name", "hammett"));
			parameters.Children.Add(new MutableConfiguration("port", "120"));

			store.AddComponentConfiguration("service", config);

			Kernel.ConfigurationStore = store;

			Container.Register(Component.For<A>().Named("a"),
			                   Component.For<ServiceUser2>().Named("service"));

			var service = Container.Resolve<ServiceUser2>("service");

			Assert.IsNotNull(service);
			Assert.IsNotNull(service.AComponent);
			Assert.IsNull(service.BComponent);
			Assert.IsNull(service.CComponent);
			Assert.AreEqual("hammett", service.Name);
			Assert.AreEqual(120, service.Port);
		}

		[Test]
		public void ParametersAndServicesBestCase2()
		{
			var store = new DefaultConfigurationStore();

			var config = new MutableConfiguration("component");
			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);
			parameters.Children.Add(new MutableConfiguration("name", "hammett"));
			parameters.Children.Add(new MutableConfiguration("port", "120"));
			parameters.Children.Add(new MutableConfiguration("Scheduleinterval", "22"));

			store.AddComponentConfiguration("service", config);

			Kernel.ConfigurationStore = store;

			Container.Register(Component.For<A>().Named("a"),
			                   Component.For<ServiceUser2>().Named("service"));

			var service = Container.Resolve<ServiceUser2>("service");

			Assert.IsNotNull(service);
			Assert.IsNotNull(service.AComponent);
			Assert.IsNull(service.BComponent);
			Assert.IsNull(service.CComponent);
			Assert.AreEqual("hammett", service.Name);
			Assert.AreEqual(120, service.Port);
			Assert.AreEqual(22, service.ScheduleInterval);
		}

		[Test]
		public void Two_constructors_but_one_with_satisfiable_dependencies()
		{
			Container.Register(Component.For<SimpleComponent1>(),
			                   Component.For<SimpleComponent2>(),
			                   Component.For<HasTwoConstructors3>());
			var component = Container.Resolve<HasTwoConstructors3>();
			Assert.IsNotNull(component.X);
			Assert.IsNotNull(component.Y);
			Assert.IsNull(component.A);
		}

		[Test]
		public void Two_constructors_but_one_with_satisfiable_dependencies_issue_IoC_209()
		{
			Container.Register(Component.For<SimpleComponent1>(),
			                   Component.For<SimpleComponent2>(),
			                   Component.For<HasTwoConstructors4>());

			Container.Resolve<HasTwoConstructors4>();
		}

		[Test]
		public void Two_constructors_but_one_with_satisfiable_dependencies_registering_dependencies_last()
		{
			Container.Register(Component.For<HasTwoConstructors3>(),
			                   Component.For<SimpleComponent1>(),
			                   Component.For<SimpleComponent2>());
			var component = Container.Resolve<HasTwoConstructors3>();
			Assert.IsNotNull(component.X);
			Assert.IsNotNull(component.Y);
			Assert.IsNull(component.A);
		}

		[Test]
		public void Two_constructors_equal_number_of_parameters_pick_one_that_can_be_satisfied()
		{
			Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
			                   Component.For<HasTwoConstructors>());

			Container.Resolve<HasTwoConstructors>();
		}

		[Test]
		public void Two_satisfiable_constructors_equal_number_of_inline_parameters_pick_one_with_more_service_overrides()
		{
			Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("Mucha"),
			                   Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("Stefan"),
			                   Component.For<HasTwoConstructors>().Named("first")
			                   	.DependsOn(ServiceOverride.ForKey("customer").Eq("Stefan")),
			                   Component.For<HasTwoConstructors>().Named("second")
			                   	.DependsOn(ServiceOverride.ForKey("common").Eq("Mucha")));

			var first = Container.Resolve<HasTwoConstructors>("first");
			var second = Container.Resolve<HasTwoConstructors>("second");

			Assert.IsNotNull(first.Customer);
			Assert.IsNotNull(second.Common);
		}

		[Test]
		public void Two_satisfiable_constructors_identical_dependency_kinds_pick_based_on_parameter_names()
		{
			Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
			                   Component.For<ICustomer>().ImplementedBy<CustomerImpl>(),
			                   Component.For<HasTwoConstructors>());

			var component = Container.Resolve<HasTwoConstructors>();

			// common is 'smaller' so we pick ctor with dependency named 'common'
			Assert.Less("common", "customer");
			Assert.IsNotNull(component.Common);
		}

		[Test]
		public void Two_satisfiable_constructors_pick_one_with_more_inline_parameters()
		{
			Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
			                   Component.For<HasTwoConstructors2>()
			                   	.DependsOn(Parameter.ForKey("param").Eq("foo")));

			var component = Container.Resolve<HasTwoConstructors2>();

			Assert.AreEqual("foo", component.Param);
		}

		[Test]
		public void When_no_constructor_is_resolvable_and_inline_arguments_are_used_HandlerException_is_thrown()
		{
			Container.Register(Component.For<ClassWithConstructors>());

			var fakeArgument = new Arguments(new[] { new object() });

			Assert.Throws<HandlerException>(() => Container.Resolve<ClassWithConstructors>(fakeArgument));
		}

		[Test]
		public void When_no_constructor_is_resolvable_and_no_inline_arguments_used_NoResolvableConstructorFoundException_is_thrown()
		{
			Container.Register(Component.For<ClassWithConstructors>());

			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<ClassWithConstructors>());

			var message =
				string.Format(
					"Can't create component 'Castle.MicroKernel.Tests.Configuration.Components.ClassWithConstructors' as it has dependencies to be satisfied.{0}{0}'Castle.MicroKernel.Tests.Configuration.Components.ClassWithConstructors' is waiting for the following dependencies:{0}- Parameter 'host' which was not provided. Did you forget to set the dependency?{0}- Parameter 'hosts' which was not provided. Did you forget to set the dependency?{0}",
					Environment.NewLine);
			Assert.AreEqual(message, exception.Message);
		}
	}
}