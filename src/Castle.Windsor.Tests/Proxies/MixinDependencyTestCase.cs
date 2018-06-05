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

namespace CastleTests.Proxies
{
	using System.Linq;

	using Castle.DynamicProxy;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;
	using CastleTests.Interceptors;

	using NUnit.Framework;

	public class MixinDependencyTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Bound_mixin_reused_as_implicit_and_explicit_dependency_chain()
		{
			Container.Register(
				Component.For<CollectInvocationsInterceptor>().LifestyleTransient(),
				Component.For<IComponent>()
					.ImplementedBy<TrivialComponent>()
					.LifestyleBoundToNearest<IUse<IComponent>>(),
				Component.For<IUse<IComponent>, UseChain<IComponent>>()
					.ImplementedBy<UseChain<IComponent>>()
					.Proxy.MixIns(m => m.Component<TrivialComponent>())
					.Interceptors<CollectInvocationsInterceptor>(),
				Component.For<IUse<IComponent>>()
					.ImplementedBy<Use<IComponent>>()
					.Proxy.MixIns(m => m.Component<TrivialComponent>())
					.Interceptors<CollectInvocationsInterceptor>()
				);

			var outerProxy = (UseChain<IComponent>)Container.Resolve<IUse<IComponent>>();
			var innerProxy = outerProxy.Next;

			var id = (outerProxy as IComponent).ID; // to trigger interception;
			id = (innerProxy as IComponent).ID; // to trigger interception;

			var outerMixin = ((outerProxy as IProxyTargetAccessor).GetInterceptors()[0] as CollectInvocationsInterceptor).Invocations[0].InvocationTarget;
			var innerMixin = ((innerProxy as IProxyTargetAccessor).GetInterceptors()[0] as CollectInvocationsInterceptor).Invocations[0].InvocationTarget;

			Assert.AreNotSame(innerMixin, outerMixin);
			Assert.AreSame(outerMixin, outerProxy.Dependency);
			Assert.AreSame(innerMixin, innerProxy.Dependency);
		}

		[Test]
		public void Bound_mixin_reused_as_implicit_and_explicit_dependency_simple()
		{
			Container.Register(
				Component.For<CollectInvocationsInterceptor>(),
				Component.For<IComponent>()
					.ImplementedBy<TrivialComponent>()
					.LifestyleBoundTo<IUse<IComponent>>(),
				Component.For<IUse<IComponent>>()
					.ImplementedBy<Use<IComponent>>()
					.Proxy.MixIns(m => m.Component<TrivialComponent>())
					.Interceptors<CollectInvocationsInterceptor>());

			var proxy = Container.Resolve<IUse<IComponent>>();
			var id = (proxy as IComponent).ID; // to trigger interception;
			var interceptor = Container.Resolve<CollectInvocationsInterceptor>();
			var mixin = interceptor.Invocations.Single().InvocationTarget;
			Assert.AreSame(mixin, proxy.Dependency);
		}
	}
}