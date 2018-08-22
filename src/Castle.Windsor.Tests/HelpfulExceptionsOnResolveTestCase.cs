// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Bugs;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Configuration.Components;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class HelpfulExceptionsOnResolveTestCase : AbstractContainerTestCase
	{
		[Test]
		public void No_resolvable_constructor_no_inline_arguments()
		{
			Container.Register(Component.For<ClassWithConstructors>());

			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<ClassWithConstructors>());

			var message =
				string.Format(
					"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}" +
					"'{1}' is waiting for the following dependencies:{0}" +
					"- Parameter 'host' which was not provided. Did you forget to set the dependency?{0}" +
					"- Parameter 'hosts' which was not provided. Did you forget to set the dependency?{0}",
					Environment.NewLine,
					typeof(ClassWithConstructors));
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		[Bug("IOC-141")]
		public void No_resolvable_constructor_open_generic_component()
		{
			Container.Register(Component.For(typeof(IoC_141.IProcessor<>)).ImplementedBy(typeof(IoC_141.DefaultProcessor<>)).Named("processor"),
			                   Component.For<IoC_141.IAssembler<object>>().ImplementedBy<IoC_141.ObjectAssembler>());

			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<IoC_141.IProcessor<int>>());

			var message = string.Format(
				"Can't create component 'processor' as it has dependencies to be satisfied.{0}{0}" +
				"'processor' is waiting for the following dependencies:{0}" +
				"- Service 'Castle.MicroKernel.Tests.Bugs.IoC_141+IAssembler`1[[{1}]]' which was not registered.{0}",
				Environment.NewLine, typeof(int).AssemblyQualifiedName);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void No_resolvable_constructor_with_inline_arguments()
		{
			Container.Register(Component.For<ClassWithConstructors>());

			var fakeArgument = new Arguments().InsertTyped(new object());

			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<ClassWithConstructors>(fakeArgument));
			var message =
				string.Format(
					"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}" +
					"'{1}' is waiting for the following dependencies:{0}" +
					"- Parameter 'host' which was not provided. Did you forget to set the dependency?{0}" +
					"- Parameter 'hosts' which was not provided. Did you forget to set the dependency?{0}",
					Environment.NewLine,
					typeof(ClassWithConstructors));
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void ReleasePolicy_tracking_the_same_instance_twice_with_transient_lifestyle_and_factory_method_suggests_different_lifestyle()
		{
			var a = new ADisposable();
			Container.Register(Component.For<A>()
			                            .LifestyleTransient()
			                            .UsingFactoryMethod(() => a));

			//so we track the instance
			Container.Resolve<A>();

			var exception = Assert.Throws<ComponentActivatorException>(() => Container.Resolve<A>());

			var message =
				string.Format(
					"Instance CastleTests.Components.ADisposable of component Late bound CastleTests.Components.A is already being tracked.{0}" +
					"The factory method providing instances of the component is reusing instances, but the lifestyle of the component is Transient which requires new instance each time.{0}" +
					"In most cases it is advised for the factory method not to be handling reuse of the instances, but to chose a lifestyle that does that appropriately.{0}" +
					"Alternatively, if you do not wish for Windsor to track the objects coming from the factory change your registration to '.UsingFactoryMethod(yourFactory, managedExternally: true)'",
					Environment.NewLine);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Resolving_by_name_not_found_prints_helpful_message_many_other_options_present()
		{
			Container.Register(Component.For<A>(),
			                   Component.For<A>().Named("something"));

			var exception =
				Assert.Throws<ComponentNotFoundException>(() =>
				                                          Container.Resolve<A>("Stefan-Mucha"));

			var expected =
				string.Format(
					"Requested component named 'Stefan-Mucha' was not found in the container. Did you forget to register it?{0}" +
					"There are 2 other components supporting requested service '{1}'. Were you looking for any of them?",
					Environment.NewLine, typeof(A).FullName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		public void Resolving_by_name_not_found_prints_helpful_message_one_other_option_present()
		{
			Container.Register(Component.For<A>());

			var exception =
				Assert.Throws<ComponentNotFoundException>(() =>
				                                          Container.Resolve<A>("Stefan-Mucha"));

			var expected =
				string.Format(
					"Requested component named 'Stefan-Mucha' was not found in the container. Did you forget to register it?{0}" +
					"There is one other component supporting requested service '{1}'. Is it what you were looking for?",
					Environment.NewLine, typeof(A).FullName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		public void Resolving_by_name_not_found_prints_helpful_message_zero_other_options_present()
		{
			var exception =
				Assert.Throws<ComponentNotFoundException>(() =>
				                                          Container.Resolve<A>("Stefan-Mucha"));

			var expected =
				string.Format(
					"Requested component named 'Stefan-Mucha' was not found in the container. Did you forget to register it?{0}" +
					"There are no components supporting requested service '{1}'. You need to register components in order to be able to use them.",
					Environment.NewLine, typeof(A).FullName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		[Bug("IOC-120")]
		public void When_attemting_to_resolve_component_with_internal_ctor_should_throw_meaningfull_exception()
		{
			Container.Register(Component.For<EmptyClass>(),
			                   Component.For<HasInternalConstructor>());

			Exception exception = Assert.Throws<ComponentActivatorException>(() =>
			                                                           Container.Resolve<HasInternalConstructor>());
			var message =
#if !FEATURE_REMOTING
				string.Format("Type {0} does not have a public default constructor and could not be instantiated.",
				              typeof(HasInternalConstructor).FullName);

			exception = exception.InnerException;
#else
				string.Format(
					"Could not find a public constructor for type {1}.{0}" +
					"Windsor by default cannot instantiate types that don't expose public constructors.{0}" +
					"To expose the type as a service add public constructor, or use custom component activator.",
					Environment.NewLine,
					typeof(HasInternalConstructor).FullName);
#endif
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		[Bug("IOC-83")]
		[Bug("IOC-120")]
		public void When_attemting_to_resolve_component_with_protected_ctor_should_throw_meaningfull_exception()
		{
			Container.Register(Component.For<HasProtectedConstructor>());

			Exception exception = Assert.Throws<ComponentActivatorException>(() => Container.Resolve<HasProtectedConstructor>());
			var message =
#if !FEATURE_REMOTING
				string.Format("Type {0} does not have a public default constructor and could not be instantiated.",
							  typeof(HasProtectedConstructor).FullName);
			exception = exception.InnerException;
#else
				string.Format(
					"Could not find a public constructor for type {1}.{0}" +
					"Windsor by default cannot instantiate types that don't expose public constructors.{0}" +
					"To expose the type as a service add public constructor, or use custom component activator.",
					Environment.NewLine,
					typeof(HasProtectedConstructor).FullName);
#endif
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void When_property_setter_throws_at_resolution_time_exception_suggests_disabling_setting_the_property()
		{
			Container.Register(
				Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
				Component.For<PropertySetterThrows>());

			var exception = Assert.Throws<ComponentActivatorException>(() => Container.Resolve<PropertySetterThrows>());

			var message = string.Format("Error setting property PropertySetterThrows.CommonService in component {1}. See inner exception for more information.{0}" +
			                            "If you don't want Windsor to set this property you can do it by either decorating it with {2} or via registration API.{0}" +
			                            "Alternatively consider making the setter non-public.",
			                            Environment.NewLine,
			                            typeof(PropertySetterThrows),
			                            typeof(DoNotWireAttribute).Name);

			Assert.AreEqual(message, exception.Message);
		}
	}
}