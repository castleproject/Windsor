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

namespace CastleTests.Proxies
{
	using System;

	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class InterceptorDependenciesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_depend_on_the_same_interceptor_multiple_times_typed()
		{
			Container.Register(
				Component.For<CountingInterceptor>(),
				Component.For<CalculatorService>()
					.Interceptors<CountingInterceptor, CountingInterceptor>()
					.Interceptors<CountingInterceptor, CountingInterceptor>());

			var calc = Container.Resolve<CalculatorService>();
			var interceptor = Container.Resolve<CountingInterceptor>();

			calc.Sum(24, 42);

			Assert.AreEqual(4, interceptor.InterceptedCallsCount);
		}
		[Test]
		public void Can_depend_on_the_same_interceptor_multiple_times_named()
		{
			Container.Register(
				Component.For<CountingInterceptor>().Named("counting"),
				Component.For<CalculatorService>()
					.Interceptors("counting", "counting")
					.Interceptors("counting", "counting"));

			var calc = Container.Resolve<CalculatorService>();
			var interceptor = Container.Resolve<CountingInterceptor>();

			calc.Sum(24, 42);

			Assert.AreEqual(4, interceptor.InterceptedCallsCount);
		}

		[Test]
		public void Missing_interceptor_by_name_throws_corrent_exception()
		{
			Container.Register(Component.For<A>().Interceptors("fooInterceptor"));
			var exception =
				Assert.Throws<HandlerException>(() =>
				                                Container.Resolve<A>());
			var message =
				string.Format(
					"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Component 'fooInterceptor' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
					Environment.NewLine,
					typeof(A).FullName);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Missing_interceptor_by_type_throws_corrent_exception()
		{
			Container.Register(Component.For<A>().Interceptors<ReturnDefaultInterceptor>());
			var exception =
				Assert.Throws<HandlerException>(() =>
				                                Container.Resolve<A>());
			var message =
				string.Format(
					"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Component '{2}' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
					Environment.NewLine,
					typeof(A).FullName,
					typeof(ReturnDefaultInterceptor).FullName);

			Assert.AreEqual(message, exception.Message);
		}
	}
}