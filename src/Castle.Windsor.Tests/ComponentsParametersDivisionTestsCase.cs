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

namespace Castle.MicroKernel.Tests
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Components;

	using CastleTests;

	using NUnit.Framework;

	[TestFixture]
	[Ignore("This is broken. Arbitrary division into Service/ServiceOverride/Parameter dependencies has to be removed or adjusted first.")]
	public class ComponentsParametersDivisionTestsCase : AbstractContainerTestCase
	{
		[Test]
		public void Collection_should_be_resolvable_normally_named()
		{
			Container.AddFacility<FactorySupportFacility>();

			Container.Register(Component.For<IEnumerable<IEmptyService>>().Named("samples")
			                   	.UsingFactoryMethod(() => new IEmptyService[]
			                   	{
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA()
			                   	}),
			                   Component.For<EnumerableDepAsConstructor>());

			var collection = Container.Resolve<IEnumerable<IEmptyService>>();
			Assert.AreEqual(10, collection.Count());

			var service = Container.Resolve<EnumerableDepAsConstructor>();

			Assert.AreEqual(10, service.Services.Count());
		}

		[Test]
		public void Collection_should_be_resolvable_normally_service_override()
		{
			Container.AddFacility<FactorySupportFacility>();

			Container.Register(Component.For<IEnumerable<IEmptyService>>()
			                   	.UsingFactoryMethod(() => new IEmptyService[]
			                   	{
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA()
			                   	}),
			                   Component.For<EnumerableDepAsConstructor>()
			                   	.ServiceOverrides(ServiceOverride
			                   	                  	.ForKey<IEnumerable<IEmptyService>>()
			                   	                  	.Eq<IEnumerable<IEmptyService>>()));

			var collection = Container.Resolve<IEnumerable<IEmptyService>>();
			Assert.AreEqual(10, collection.Count());

			var service = Container.Resolve<EnumerableDepAsConstructor>();

			Assert.AreEqual(10, service.Services.Count());
		}

		[Test]
		public void Collection_should_be_resolvable_normally_service_override_named()
		{
			Container.AddFacility<FactorySupportFacility>();

			Container.Register(Component.For<IEnumerable<IEmptyService>>().Named("foo")
			                   	.UsingFactoryMethod(() => new IEmptyService[]
			                   	{
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA()
			                   	}),
			                   Component.For<EnumerableDepAsConstructor>()
			                   	.ServiceOverrides(ServiceOverride
			                   	                  	.ForKey<IEnumerable<IEmptyService>>()
			                   	                  	.Eq("foo")));

			var collection = Container.Resolve<IEnumerable<IEmptyService>>();
			Assert.AreEqual(10, collection.Count());

			var service = Container.Resolve<EnumerableDepAsConstructor>();

			Assert.AreEqual(10, service.Services.Count());
		}

		[Test]
		public void Collection_should_be_resolvable_normally_typed()
		{
			Container.AddFacility<FactorySupportFacility>();

			Container.Register(Component.For<IEnumerable<IEmptyService>>()
			                   	.UsingFactoryMethod(() => new IEmptyService[]
			                   	{
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA(),
			                   		new EmptyServiceA()
			                   	}),
			                   Component.For<EnumerableDepAsConstructor>());

			var collection = Container.Resolve<IEnumerable<IEmptyService>>();
			Assert.AreEqual(10, collection.Count());

			var service = Container.Resolve<EnumerableDepAsConstructor>();

			Assert.AreEqual(10, service.Services.Count());
		}
	}
}