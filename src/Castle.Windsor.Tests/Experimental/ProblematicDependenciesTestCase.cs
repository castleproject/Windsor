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
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Experimental.Diagnostics;
	using Castle.Windsor.Experimental.Diagnostics.DebuggerViews;
	using Castle.Windsor.Experimental.Diagnostics.Extensions;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	public class ProblematicDependenciesTestCase : AbstractContainerTestFixture
	{
		private DefaultDebuggingSubSystem subSystem;

		[Test]
		public void Can_detect_singleton_depending_on_transient()
		{
			Container.Register(Component.For<B>().LifeStyle.Singleton,
			                   Component.For<A>().LifeStyle.Transient);

			var mismatches = GetMismatches();
			Assert.AreEqual(1, mismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient_directly_and_indirectly()
		{
			Container.Register(Component.For<CBA>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Singleton,
			                   Component.For<A>().LifeStyle.Transient);

			var items = GetMismatches();
			Assert.AreEqual(3, items.Length);
			var cbaMismatches = items.Where(i => i.Handlers.First().Services.Single() == typeof(CBA)).ToArray();
			Assert.AreEqual(2, cbaMismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient_indirectly()
		{
			Container.Register(Component.For<C>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Singleton,
			                   Component.For<A>().LifeStyle.Transient);

			var mismatches = GetMismatches();
			Assert.AreEqual(2, mismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient_indirectly_via_custom_lifestyle()
		{
			Container.Register(Component.For<C>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Custom<CustomLifestyleManager>(),
			                   Component.For<A>().LifeStyle.Transient);

			var mismatches = GetMismatches();
			Assert.AreEqual(1, mismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_two_transients_directly_and_indirectly()
		{
			Container.Register(Component.For<CBA>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Transient,
			                   Component.For<A>().LifeStyle.Transient);

			var items = GetMismatches();
			Assert.AreEqual(2, items.Length);
			var cbaMismatches = items.Where(i => i.Handlers.First().Services.Single() == typeof(CBA)).ToArray();
			Assert.AreEqual(2, cbaMismatches.Length);
		}

		[SetUp]
		public void SetSubSystem()
		{
#if SILVERLIGHT
			Init();
#endif
			subSystem = new DefaultDebuggingSubSystem();
			Kernel.AddSubSystem(SubSystemConstants.DebuggingKey, subSystem);
		}

		private MismatchedDependencyDebuggerViewItem[] GetMismatches()
		{
			var faultyComponents =
				subSystem.SelectMany(e => e.Attach()).SingleOrDefault(i => i.Name == PotentialLifestyleMismatches.Name);
			Assert.IsNotNull(faultyComponents);
			var components = faultyComponents.Value as DebuggerViewItem[];
			Assert.IsNotNull(components);
			return components.Select(i => (MismatchedDependencyDebuggerViewItem)i.Value).ToArray();
		}
	}
#endif
}