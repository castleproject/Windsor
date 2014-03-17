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
	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class InterceptorsSelectorTestCase
	{
		[Test]
		public void CanApplyInterceptorsToSelectedMethods()
		{
			IWindsorContainer container = new WindsorContainer();
			container.Register(
				Component.For<ICatalog>()
					.ImplementedBy<SimpleCatalog>()
					.Interceptors(InterceptorReference.ForType<WasCalledInterceptor>())
					.SelectedWith(new DummyInterceptorSelector()).Anywhere,
				Component.For<WasCalledInterceptor>()
				);

			Assert.IsFalse(WasCalledInterceptor.WasCalled);

			var catalog = container.Resolve<ICatalog>();
			catalog.AddItem("hot dogs");
			Assert.IsTrue(WasCalledInterceptor.WasCalled);

			WasCalledInterceptor.WasCalled = false;
			catalog.RemoveItem("hot dogs");
			Assert.IsFalse(WasCalledInterceptor.WasCalled);
		}
	}

	public interface ICatalog
	{
		void AddItem(object item);

		void RemoveItem(object item);
	}

	public class SimpleCatalog : ICatalog
	{
		public void AddItem(object item)
		{
		}

		public void RemoveItem(object item)
		{
		}
	}
}