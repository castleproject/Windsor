// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Pools
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Lifestyle.Pool;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.Tests.Pools;
	using Castle.Windsor;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class PooledLifestyleManagerTestCase : AbstractContainerTestCase
	{
		public class DisposableMockObject : IDisposable
		{
			private readonly Action disposeAction;

			public DisposableMockObject(Action disposeAction)
			{
				this.disposeAction = disposeAction;
			}

			public void Dispose()
			{
				disposeAction();
			}
		}

		[Test]
		public void DisposePoolDisposesTrackedComponents()
		{
			// Arrange.
			var result = false;
			var container = new WindsorContainer();
			container.Register(Component.For<DisposableMockObject>().LifestylePooled(1, 5));
			container.Release(container.Resolve<DisposableMockObject>(new Arguments().InsertProperties(new
			{
				disposeAction = new Action(() => { result = true; })
			})));

			// Act.
			container.Dispose();

			// Assert.
			Assert.IsTrue(result);
		}

		[Test]
		public void MaxSize()
		{
			Kernel.Register(Component.For<PoolableComponent1>());

			var instances = new List<PoolableComponent1>
			{
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>()
			};

			var other1 = Kernel.Resolve<PoolableComponent1>();

			CollectionAssert.DoesNotContain(instances, other1);

			foreach (var inst in instances)
			{
				Kernel.ReleaseComponent(inst);
			}

			Kernel.ReleaseComponent(other1);

			var other2 = Kernel.Resolve<PoolableComponent1>();

			Assert.AreNotEqual(other1, other2);
			CollectionAssert.Contains(instances, other2);

			Kernel.ReleaseComponent(other2);
		}

		[Test]
		public void Poolable_component_is_always_tracked()
		{
			Kernel.Register(Component.For<A>().LifeStyle.Pooled);

			var component = Kernel.Resolve<A>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Recyclable_component_as_dependency_can_be_reused()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null),
				Component.For<UseRecyclableComponent>().LifeStyle.Transient);
			var component = Kernel.Resolve<UseRecyclableComponent>();
			Container.Release(component);
			var componentAgain = Kernel.Resolve<UseRecyclableComponent>();

			Assert.AreSame(componentAgain.Dependency, component.Dependency);

			Container.Release(componentAgain);
			Assert.AreEqual(2, componentAgain.Dependency.RecycledCount);
		}

		[Test]
		public void Recyclable_component_can_be_reused()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifestylePooled(1));
			var component = Kernel.Resolve<RecyclableComponent>();
			Container.Release(component);
			var componentAgain = Kernel.Resolve<RecyclableComponent>();

			Assert.AreSame(componentAgain, component);

			Container.Release(componentAgain);
			Assert.AreEqual(2, componentAgain.RecycledCount);
		}

		[Test]
		public void Recyclable_component_gets_recycled_just_once_on_subsequent_release()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null));
			var component = Kernel.Resolve<RecyclableComponent>();

			Assert.AreEqual(0, component.RecycledCount);

			Container.Release(component);
			Container.Release(component);
			Container.Release(component);
			Assert.AreEqual(1, component.RecycledCount);
		}

		[Test]
		public void Recyclable_component_gets_recycled_on_release()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null));
			var component = Kernel.Resolve<RecyclableComponent>();

			Assert.AreEqual(0, component.RecycledCount);

			Container.Release(component);
			Assert.AreEqual(1, component.RecycledCount);
		}

		[Test]
		public void Recyclable_component_with_on_release_action_not_released_more_than_necessary()
		{
			var count = 0;
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null)
				.DynamicParameters((k, d) =>
				{
					count++;
					return delegate { count--; };
				}));

			RecyclableComponent component = null;
			for (var i = 0; i < 10; i++)
			{
				component = Kernel.Resolve<RecyclableComponent>();
				Container.Release(component);
			}
			Assert.AreEqual(10, component.RecycledCount);
			Assert.AreEqual(0, count);
		}

		[Test]
		public void SimpleUsage()
		{
			Kernel.Register(Component.For<PoolableComponent1>());

			var inst1 = Kernel.Resolve<PoolableComponent1>();
			var inst2 = Kernel.Resolve<PoolableComponent1>();

			Kernel.ReleaseComponent(inst2);
			Kernel.ReleaseComponent(inst1);

			var other1 = Kernel.Resolve<PoolableComponent1>();
			var other2 = Kernel.Resolve<PoolableComponent1>();

			Assert.AreSame(inst1, other1);
			Assert.AreSame(inst2, other2);

			Kernel.ReleaseComponent(inst2);
			Kernel.ReleaseComponent(inst1);
		}

		[Test]
		public void Parallel_usage_only_registers_single_factory()
		{
			using (AutoResetEvent evt = new AutoResetEvent(false))
			{
				ParallelAccessAwareKernel kernel = new ParallelAccessAwareKernel(evt);

				var manager1 = new MyPoolableLifestyleManager();
				var manager2 = new MyPoolableLifestyleManager();

				manager1.Init(null, kernel, null);
				manager2.Init(null, kernel, null);

				ThreadPool.QueueUserWorkItem(o => { manager1.CreatePool(); });
				ThreadPool.QueueUserWorkItem(o => { manager2.CreatePool(); });

				Thread.Sleep(TimeSpan.FromSeconds(1));
				evt.Set();
				Thread.Sleep(TimeSpan.FromSeconds(1));

				Assert.That(kernel.parallelCount, Is.EqualTo(0));
			}
		}

		private sealed class MyPoolableLifestyleManager : PoolableLifestyleManager
		{
			public MyPoolableLifestyleManager()
				: base(1, 2)
			{ }

			public void CreatePool()
			{
				base.CreatePool(2, 5).Dispose();
			}
		}

		private sealed class ParallelAccessAwareKernel : IKernel
		{
			private readonly AutoResetEvent evt;
#pragma warning disable 0067
			public event ComponentDataDelegate ComponentRegistered;
			public event ComponentModelDelegate ComponentModelCreated;
			public event EventHandler AddedAsChildKernel;
			public event EventHandler RemovedAsChildKernel;
			public event ComponentInstanceDelegate ComponentCreated;
			public event ComponentInstanceDelegate ComponentDestroyed;
			public event HandlerDelegate HandlerRegistered;
			public event HandlersChangedDelegate HandlersChanged;
			public event DependencyDelegate DependencyResolving;
			public event EventHandler RegistrationCompleted;
			public event ServiceDelegate EmptyCollectionResolving;
#pragma warning restore 0067

			public ParallelAccessAwareKernel(AutoResetEvent evt)
			{
				this.evt = evt;
			}

			public void Dispose()
			{
			}

			public void AddComponent(string key, Type classType)
			{
				throw new NotImplementedException();
			}

			public void AddComponent(string key, Type classType, LifestyleType lifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent(string key, Type classType, LifestyleType lifestyle, bool overwriteLifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent(string key, Type serviceType, Type classType)
			{
				throw new NotImplementedException();
			}

			public void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle, bool overwriteLifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent<T>()
			{
				throw new NotImplementedException();
			}

			public void AddComponent<T>(LifestyleType lifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent<T>(LifestyleType lifestyle, bool overwriteLifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent<T>(Type serviceType)
			{
				throw new NotImplementedException();
			}

			public void AddComponent<T>(Type serviceType, LifestyleType lifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponent<T>(Type serviceType, LifestyleType lifestyle, bool overwriteLifestyle)
			{
				throw new NotImplementedException();
			}

			public void AddComponentInstance<T>(object instance)
			{
				throw new NotImplementedException();
			}

			public void AddComponentInstance<T>(Type serviceType, object instance)
			{
				throw new NotImplementedException();
			}

			public void AddComponentInstance(string key, object instance)
			{
				throw new NotImplementedException();
			}

			public void AddComponentInstance(string key, Type serviceType, object instance)
			{
				throw new NotImplementedException();
			}

			public void AddComponentInstance(string key, Type serviceType, Type classType, object instance)
			{
				throw new NotImplementedException();
			}

			public void AddComponentWithExtendedProperties(string key, Type classType, IDictionary extendedProperties)
			{
				throw new NotImplementedException();
			}

			public void AddComponentWithExtendedProperties(string key, Type serviceType, Type classType, IDictionary extendedProperties)
			{
				throw new NotImplementedException();
			}

			public IKernel AddFacility(string key, IFacility facility)
			{
				throw new NotImplementedException();
			}

			public IKernel AddFacility<T>(string key) where T : IFacility, new()
			{
				throw new NotImplementedException();
			}

			public IKernel AddFacility<T>(string key, Action<T> onCreate) where T : IFacility, new()
			{
				throw new NotImplementedException();
			}

			public object Resolve(string key, object argumentsAsAnonymousType)
			{
				throw new NotImplementedException();
			}

			public object Resolve(string key, IDictionary arguments)
			{
				throw new NotImplementedException();
			}

			public IComponentModelBuilder ComponentModelBuilder { get; private set; }
			public IConfigurationStore ConfigurationStore { get; set; }
			public GraphNode[] GraphNodes { get; private set; }
			public IHandlerFactory HandlerFactory { get; private set; }
			public IKernel Parent { get; set; }
			public IProxyFactory ProxyFactory { get; set; }
			public IReleasePolicy ReleasePolicy { get; set; }
			public IDependencyResolver Resolver { get; private set; }

			public void AddChildKernel(IKernel kernel)
			{
				throw new NotImplementedException();
			}

			public IKernel AddFacility(IFacility facility)
			{
				throw new NotImplementedException();
			}

			public IKernel AddFacility<T>() where T : IFacility, new()
			{
				throw new NotImplementedException();
			}

			public IKernel AddFacility<T>(Action<T> onCreate) where T : IFacility, new()
			{
				throw new NotImplementedException();
			}

			public void AddHandlerSelector(IHandlerSelector selector)
			{
				throw new NotImplementedException();
			}

			public void AddHandlersFilter(IHandlersFilter filter)
			{
				throw new NotImplementedException();
			}

			public void AddSubSystem(string name, ISubSystem subsystem)
			{
				throw new NotImplementedException();
			}

			public IHandler[] GetAssignableHandlers(Type service)
			{
				throw new NotImplementedException();
			}

			public IFacility[] GetFacilities()
			{
				throw new NotImplementedException();
			}

			public IHandler GetHandler(string name)
			{
				throw new NotImplementedException();
			}

			public IHandler[] GetHandlers()
			{
				throw new NotImplementedException();
			}

			public IHandler GetHandler(Type service)
			{
				throw new NotImplementedException();
			}

			public IHandler[] GetHandlers(Type service)
			{
				throw new NotImplementedException();
			}

			public ISubSystem GetSubSystem(string name)
			{
				throw new NotImplementedException();
			}

			public bool HasComponent(string name)
			{
				throw new NotImplementedException();
			}

			public int parallelCount = 0;
			private bool registered;

			public bool HasComponent(Type service)
			{
				return registered;
			}

			public IKernel Register(params IRegistration[] registrations)
			{
				Interlocked.Increment(ref parallelCount);
				evt.WaitOne();
				registered = true;
				Interlocked.CompareExchange(ref parallelCount, 0, 1);
				return null;
			}

			public void ReleaseComponent(object instance)
			{
				throw new NotImplementedException();
			}

			public void RemoveChildKernel(IKernel kernel)
			{
				throw new NotImplementedException();
			}

			public object Resolve(Type service)
			{
				throw new NotImplementedException();
			}

			public object Resolve(Type service, Arguments arguments)
			{
				throw new NotImplementedException();
			}

			public object Resolve(string key, Type service)
			{
				throw new NotImplementedException();
			}

			public T Resolve<T>(Arguments arguments)
			{
				throw new NotImplementedException();
			}

			public T Resolve<T>()
			{
				return (T)(object)new EmptyPoolFactory();
			}

			public T Resolve<T>(string key)
			{
				throw new NotImplementedException();
			}

			public T Resolve<T>(string key, Arguments arguments)
			{
				throw new NotImplementedException();
			}

			public object Resolve(string key, Type service, Arguments arguments)
			{
				throw new NotImplementedException();
			}

			public Array ResolveAll(Type service)
			{
				throw new NotImplementedException();
			}

			public Array ResolveAll(Type service, Arguments arguments)
			{
				throw new NotImplementedException();
			}

			public TService[] ResolveAll<TService>()
			{
				throw new NotImplementedException();
			}

			public TService[] ResolveAll<TService>(Arguments arguments)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class EmptyPoolFactory : IPoolFactory
		{
			public IPool Create(int initialsize, int maxSize, IComponentActivator activator)
			{
				return new EmptyPool();
			}
		}

		private sealed class EmptyPool : IPool
		{
			public void Dispose()
			{ }

			public bool Release(object instance)
			{
				throw new NotImplementedException();
			}

			public object Request(CreationContext context, Func<CreationContext, Burden> creationCallback)
			{
				throw new NotImplementedException();
			}
		}
	}
}
