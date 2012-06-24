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
	using System.Collections.Generic;
	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Registration;
	using CastleTests.Components;
	using NUnit.Framework;

	[TestFixture]
	public class KernelEvents_ComponentCreated_TestCase : AbstractContainerTestCase
	{
		readonly List<KeyValuePair<ComponentModel, object>> list = new List<KeyValuePair<ComponentModel, object>>();

		protected override void AfterContainerCreated()
		{
			list.Clear();
			Container.Kernel.ComponentCreated += Kernel_ComponentCreated;
		}

		void Kernel_ComponentCreated(ComponentModel model, object instance)
		{
			list.Add(new KeyValuePair<ComponentModel, object>(model, instance));
		}


		[Test]
		[Description("As reported in http://stackoverflow.com/questions/8923931/castle-windsor-component-created-event-with-interceptor")]
		public void Event_raised_for_component_with_interceptor()
		{

			Container.Register(
				Component.For<IInterceptor>().ImplementedBy<StandardInterceptor>().LifestyleTransient(),
				Component.For<IService>().ImplementedBy<MyService>().Interceptors<StandardInterceptor>().LifestyleTransient());

			var service = Container.Resolve<IService>();
			Assert.IsNotEmpty(list);
			Assert.IsTrue(list.Exists(t => t.Value == service));
		}
	}
}