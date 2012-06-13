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

namespace Castle.MicroKernel.Tests
{
	using System;
	using System.Collections;
	using System.Threading;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class LazyLoadingTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_Lazily_resolve_component()
		{
			Container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LoaderForDefaultImplementations>());
			var service = Container.Resolve("foo", typeof(IHasDefaultImplementation));
			Assert.IsNotNull(service);
			Assert.IsInstanceOf<Implementation>(service);
		}

		[Test]
		public void Can_lazily_resolve_dependency()
		{
			Container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LoaderForDefaultImplementations>(),
			                   Component.For<UsingLazyComponent>());
			var component = Container.Resolve<UsingLazyComponent>();
			Assert.IsNotNull(component.Dependency);
		}

		[Test]
		public void Can_lazily_resolve_explicit_dependency()
		{
			Container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LoaderUsingDependency>());
			var component = Container.Resolve<UsingString>(new Arguments().Insert("parameter", "Hello"));
			Assert.AreEqual("Hello", component.Parameter);
		}

		[Test]
		public void Component_loaded_lazily_can_have_lazy_dependencies()
		{
			Container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<ABLoader>());
			Container.Resolve<B>();
		}

		[Test]
		[Timeout(2000)]
		public void Loaders_are_thread_safe()
		{
			Container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<SlowLoader>());
			var @event = new ManualResetEvent(false);
			int[] count = { 10 };
			Exception exception = null;
			for (var i = 0; i < count[0]; i++)
			{
				ThreadPool.QueueUserWorkItem(o =>
				{
					try
					{
						Container.Resolve<Implementation>("not registered");
						if (Interlocked.Decrement(ref count[0]) == 0)
						{
							@event.Set();
						}
					}
					catch (Exception e)
					{
						exception = e;
						// this is required because NUnit does not consider it a failure when
						// an exception is thrown on a non-main thread and therfore it waits.
						@event.Set();
					}
				}
					);
			}
			@event.WaitOne();
			Assert.IsNull(exception);
			Assert.AreEqual(0, count[0]);
		}

		[Test]
		public void Loaders_only_triggered_when_resolving()
		{
			var loader = new ABLoaderWithGuardClause();
			Container.Register(Component.For<ILazyComponentLoader>().Instance(loader),
			                   Component.For<B>());

			loader.CanLoadNow = true;

			Container.Resolve<B>();
		}

		[Test]
		public void Loaders_with_dependencies_dont_overflow_the_stack()
		{
			Container.Register(Component.For<LoaderWithDependency>());

			Assert.Throws<ComponentNotFoundException>(() =>
			                                          Container.Resolve<ISpecification>("some not registered service"));
		}
	}

	public class ABLoaderWithGuardClause : ILazyComponentLoader
	{
		public bool CanLoadNow { get; set; }

		public IRegistration Load(string name, Type service, IDictionary arguments)
		{
			Assert.True(CanLoadNow);

			if (service == typeof(A) || service == typeof(B))
			{
				return Component.For(service);
			}
			return null;
		}
	}

	public class ABLoader : ILazyComponentLoader
	{
		public IRegistration Load(string name, Type service, IDictionary arguments)
		{
			if (service == typeof(A) || service == typeof(B))
			{
				return Component.For(service);
			}
			return null;
		}
	}

	public class LoaderWithDependency : ILazyComponentLoader
	{
		private IEmployee employee;

		public LoaderWithDependency(IEmployee employee)
		{
			this.employee = employee;
		}

		public IRegistration Load(string name, Type service, IDictionary arguments)
		{
			return null;
		}
	}

	public class SlowLoader : ILazyComponentLoader
	{
		public IRegistration Load(string name, Type service, IDictionary argume)
		{
			Thread.Sleep(200);
			return Component.For(service).Named(name);
		}
	}

	public class LoaderForDefaultImplementations : ILazyComponentLoader
	{
		public IRegistration Load(string name, Type service, IDictionary arguments)
		{
			if (!Attribute.IsDefined(service, typeof(DefaultImplementationAttribute)))
			{
				return null;
			}

			var attributes = service.GetCustomAttributes(typeof(DefaultImplementationAttribute), false);
			var attribute = attributes[0] as DefaultImplementationAttribute;
			return Component.For(service).ImplementedBy(attribute.Implementation).Named(name);
		}
	}

	public class LoaderUsingDependency : ILazyComponentLoader
	{
		public IRegistration Load(string name, Type service, IDictionary arguments)
		{
			return Component.For(service).DependsOn(arguments);
		}
	}

	public class UsingString
	{
		private readonly string parameter;

		public UsingString(string parameter)
		{
			this.parameter = parameter;
		}

		public string Parameter
		{
			get { return parameter; }
		}
	}

	public class UsingLazyComponent
	{
		private readonly IHasDefaultImplementation dependency;

		public UsingLazyComponent(IHasDefaultImplementation dependency)
		{
			this.dependency = dependency;
		}

		public IHasDefaultImplementation Dependency
		{
			get { return dependency; }
		}
	}

	[DefaultImplementation(typeof(Implementation))]
	public interface IHasDefaultImplementation
	{
		void Foo();
	}

	public class Implementation : IHasDefaultImplementation
	{
		public void Foo()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
	public class DefaultImplementationAttribute : Attribute
	{
		private readonly Type implementation;

		public DefaultImplementationAttribute(Type implementation)
		{
			this.implementation = implementation;
		}

		public Type Implementation
		{
			get { return implementation; }
		}
	}
}