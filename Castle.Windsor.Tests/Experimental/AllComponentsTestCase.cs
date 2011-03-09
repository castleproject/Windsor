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

namespace Castle.Windsor.Tests.Experimental
{
#if !SILVERLIGHT
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Experimental.Diagnostics;
	using Castle.Windsor.Experimental.Diagnostics.DebuggerViews;
	using Castle.Windsor.Experimental.Diagnostics.Extensions;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	public class AllComponentsTestCase : AbstractContainerTestCase
	{
		[SetUp]
		public void SetSubSystem()
		{
			subSystem = new DefaultDebuggingSubSystem();
			Kernel.AddSubSystem(SubSystemConstants.DebuggingKey, subSystem);
		}

		private DefaultDebuggingSubSystem subSystem;

		private DebuggerViewItem GetAllComponents()
		{
			return subSystem.SelectMany(e => e.Attach()).SingleOrDefault(i => i.Name == AllComponents.Name);
		}

		[Test]
		public void Forwarded_components_are_kept_together()
		{
			Container.Register(Component.For<IEmptyService, EmptyServiceA>().ImplementedBy<EmptyServiceA>().Named("A"));

			var allComponents = GetAllComponents();
			Assert.IsNotNull(allComponents);
			var view = allComponents.Value as ComponentDebuggerView[];
			Assert.IsNotNull(view);
			Assert.AreEqual(1, view.Length);
			var item = view.Single();
			Assert.IsNotNull(item.Extensions.SingleOrDefault(e => e.Name == "Service" && Equals(e.Value, typeof(EmptyServiceA))));
			Assert.IsNotNull(item.Extensions.SingleOrDefault(e => e.Name == "Service" && Equals(e.Value, typeof(IEmptyService))));
		}
	}

#endif
}