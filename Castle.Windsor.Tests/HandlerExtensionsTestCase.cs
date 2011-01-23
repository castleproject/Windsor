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

namespace Castle.Windsor.Tests
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class HandlerExtensionsTestCase:AbstractContainerTestFixture
	{
		private ComponentRegistration<A> AddResolveExtensions(ComponentRegistration<A> componentRegistration,
		                                                      params IResolveExtension[] items)
		{
			var resolveExtensions = new List<IResolveExtension>();
			foreach (var item in items.Distinct())
			{
				resolveExtensions.Add(item);
			}
			return componentRegistration.ExtendedProperties(Property.ForKey("Castle.ResolveExtensions").Eq(resolveExtensions));
		}

		private ComponentRegistration<TComponent> WithReleaseExtensions<TComponent>(
			ComponentRegistration<TComponent> componentRegistration, params IReleaseExtension[] items) 
            where TComponent : class
		{
			var releaseExtensions = new List<IReleaseExtension>();
			foreach (var item in items.Distinct())
			{
				releaseExtensions.Add(item);
			}
			return componentRegistration.ExtendedProperties(Property.ForKey("Castle.ReleaseExtensions").Eq(releaseExtensions));
		}

		[Test]
		public void Can_chain_extensions()
		{
			var a = new A();
			var collector = new CollectItemsExtension();
			Kernel.Register(AddResolveExtensions(Component.For<A>(), collector, new ReturnAExtension(a)));
			Kernel.Resolve<A>();
			Assert.AreSame(a, collector.ResolvedItems.Single());
		}

		[Test]
		public void Can_intercept_entire_resolution()
		{
			var a = new A();
			var componentRegistration = Component.For<A>();
			Kernel.Register(AddResolveExtensions(componentRegistration, new ReturnAExtension(a)));
			var resolvedA = Kernel.Resolve<A>();
			Assert.AreSame(a, resolvedA);
		}

		[Test]
		public void Can_proceed_and_inspect_released_value_on_singleton()
		{
			var collector = new CollectItemsExtension();
			Kernel.Register(WithReleaseExtensions(Component.For<DisposableFoo>(), collector));
			var a = Kernel.Resolve<DisposableFoo>();
			Kernel.Dispose();
			Assert.AreEqual(1, collector.ReleasedItems.Count);
			Assert.AreSame(a, collector.ReleasedItems[0]);
		}

		[Test]
		public void Can_proceed_and_inspect_released_value_on_transinet()
		{
			var collector = new CollectItemsExtension();
			Kernel.Register(WithReleaseExtensions(Component.For<DisposableFoo>().LifeStyle.Transient, collector));
			var a = Kernel.Resolve<DisposableFoo>();
			Kernel.ReleaseComponent(a);
			Assert.AreEqual(1, collector.ReleasedItems.Count);
			Assert.AreSame(a, collector.ReleasedItems[0]);
		}

		[Test]
		public void Can_proceed_and_inspect_returned_value()
		{
			var collector = new CollectItemsExtension();
			Kernel.Register(AddResolveExtensions(Component.For<A>(), collector));
			Kernel.Resolve<A>();
			var resolved = Kernel.Resolve<A>();
			Assert.AreEqual(2, collector.ResolvedItems.Count);
			Assert.AreSame(resolved, collector.ResolvedItems[0]);
			Assert.AreSame(resolved, collector.ResolvedItems[1]);
		}

		[Test]
		public void Can_replace_returned_value()
		{
			var a = new A();
			var componentRegistration = Component.For<A>();
			Kernel.Register(AddResolveExtensions(componentRegistration, new ReturnAExtension(a, proceed: true)));
			var resolvedA = Kernel.Resolve<A>();
			Assert.AreSame(a, resolvedA);
		}
	}

	public class ReturnAExtension : IResolveExtension
	{
		private readonly bool proceed;
		private A a;

		public ReturnAExtension(A a, bool proceed = false)
		{
			this.a = a;
			this.proceed = proceed;
		}

		public void Init(IKernel kernel, IHandler handler)
		{
			
		}

		public void Intercept(ResolveInvocation invocation)
		{
			if (proceed)
			{
				invocation.Proceed();
			}
			invocation.ResolvedInstance = a;
		}
	}

	public class CollectItemsExtension : IResolveExtension, IReleaseExtension
	{
		private readonly IList<object> releasedItems = new List<object>();
		private readonly IList<object> resolvedItems = new List<object>();

		public IList<object> ReleasedItems
		{
			get { return releasedItems; }
		}

		public IList<object> ResolvedItems
		{
			get { return resolvedItems; }
		}

		public void Intercept(ReleaseInvocation invocation)
		{
			invocation.Proceed();
			releasedItems.Add(invocation.Instance);
		}

		public void Init(IKernel kernel, IHandler handler)
		{
			
		}

		public void Intercept(ResolveInvocation invocation)
		{
			invocation.Proceed();
			resolvedItems.Add(invocation.ResolvedInstance);
		}
	}
}