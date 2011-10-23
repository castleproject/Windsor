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

namespace CastleTests
{
	using System;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class KernelEvents_EmptyCollectionResolving_TestCase : AbstractContainerTestCase
	{
		[Test]
		public void Event_NOT_raised_when_non_empty_collection_is_resolved()
		{
			Kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());

			var wasRaised = false;
			Kernel.EmptyCollectionResolving += t => { wasRaised = true; };

			var services = Container.ResolveAll<IEmptyService>();

			Assert.False(wasRaised);
			Assert.IsNotEmpty(services);
		}

		[Test]
		public void Event_NOT_raised_when_non_empty_collection_is_resolved_from_parent_container()
		{
			Kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());

			var childContainer = new WindsorContainer();
			childContainer.Parent = Container;

			var wasRaised = false;
			childContainer.Kernel.EmptyCollectionResolving += t => { wasRaised = true; };

			var services = childContainer.ResolveAll<IEmptyService>();

			Assert.False(wasRaised);
			Assert.IsNotEmpty(services);
		}

		[Test]
		public void Event_raised_when_empty_collection_is_resolved()
		{
			Type type = null;
			Kernel.EmptyCollectionResolving += t => { type = t; };

			var services = Container.ResolveAll<IEmptyService>();

			Assert.AreEqual(typeof(IEmptyService), type);
			Assert.IsEmpty(services);
		}
	}
}