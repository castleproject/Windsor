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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

	using NUnit.Framework;

	[TestFixture]
	public class HandlerExtensionsTestCase
	{
		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
		}

		private DefaultKernel kernel;

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

		private ComponentRegistration<TComponent> AddReleaseExtensions<TComponent>(
			ComponentRegistration<TComponent> componentRegistration, params IReleaseExtension[] items)
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
			kernel.Register(AddResolveExtensions(Component.For<A>(), collector, new ReturnAExtension(a)));
			kernel.Resolve<A>();
			Assert.AreSame(a, collector.ResolvedItems.Single());
		}

		[Test]
		public void Can_intercept_entire_resolution()
		{
			var a = new A();
			var componentRegistration = Component.For<A>();
			kernel.Register(AddResolveExtensions(componentRegistration, new ReturnAExtension(a)));
			var resolvedA = kernel.Resolve<A>();
			Assert.AreSame(a, resolvedA);
		}

		[Test]
		public void Can_proceed_and_inspect_released_value()
		{
			var collector = new CollectItemsExtension();
			kernel.Register(AddReleaseExtensions(Component.For<DisposableFoo>(), collector));
			var a = kernel.Resolve<DisposableFoo>();
			kernel.ReleaseComponent(a);
			Assert.AreEqual(1, collector.ReleasedItems.Count);
			Assert.AreSame(a, collector.ReleasedItems[0]);
		}

		[Test]
		public void Can_proceed_and_inspect_returned_value()
		{
			var collector = new CollectItemsExtension();
			kernel.Register(AddResolveExtensions(Component.For<A>(), collector));
			kernel.Resolve<A>();
			var resolved = kernel.Resolve<A>();
			Assert.AreEqual(2, collector.ResolvedItems.Count);
			Assert.AreSame(resolved, collector.ResolvedItems[0]);
			Assert.AreSame(resolved, collector.ResolvedItems[1]);
		}

		[Test]
		public void Can_replace_returned_value()
		{
			var a = new A();
			var componentRegistration = Component.For<A>();
			kernel.Register(AddResolveExtensions(componentRegistration, new ReturnAExtension(a, proceed: true)));
			var resolvedA = kernel.Resolve<A>();
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
			invocation.ReturnValue = a;
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
			resolvedItems.Add(invocation.ReturnValue);
		}
	}
}