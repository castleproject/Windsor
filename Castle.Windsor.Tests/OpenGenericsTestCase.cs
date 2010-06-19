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


namespace Castle.Windsor.Tests
{
	using System.Collections.ObjectModel;
	using Castle.DynamicProxy;
	using Core;
	using MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class OpenGenericsTestCase
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			container = new WindsorContainer();
		}

		#endregion

		private WindsorContainer container;

		[Test]
		public void ExtendedProperties_incl_ProxyOptions_are_honored_for_open_generic_types()
		{

			container.Kernel.Register(Component.For<StandardInterceptor>().LifeStyle.Transient);

			container.Register(Component.For(typeof(Collection<>))
								.Proxy.AdditionalInterfaces(typeof(ITestInterface))
								.Interceptors(new InterceptorReference(typeof(StandardInterceptor))).Last
								.LifeStyle.Transient);
			var tst = container.Resolve<Collection<int>>();
			var tst1 = tst as ITestInterface;
			Assert.IsNotNull(tst1);
		}

		[Test]
		public void ResolveAll_for_open_generic_with_constraints_works()
		{
			container.Register(Component.For(typeof(IIface<,>)).ImplementedBy(typeof(HandlerImpl<,>)));
			var invalid = container.ResolveAll<IIface<Impl1, Stub>>();
			Assert.AreEqual(0, invalid.Length);
			var valid = container.ResolveAll<IIface<Impl2, Stub>>();
			Assert.AreEqual(1, valid.Length);
		}
	}


	public interface ITestInterface
	{
		void Foo();
	}

	public class Base { }

	public class Stub { }

	public interface AdditionalIface
	{
	}

	public interface IIface<T1, T2>
		where T1 : Base
		where T2 : class
	{
		T1 Command { get; set; }
	}

	public class HandlerImpl<T1, T2> : IIface<T1, T2>
		where T1 : Base, AdditionalIface
		where T2 : class
	{
		public T1 Command { get; set; }
	}

	public class Impl1 : Base
	{
	}

	public class Impl2 : Base, AdditionalIface
	{
	}

}
