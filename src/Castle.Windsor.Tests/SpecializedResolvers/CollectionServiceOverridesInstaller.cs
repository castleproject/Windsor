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

namespace CastleTests.SpecializedResolvers
{
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;

	using CastleTests.Components;

	internal class CollectionServiceOverridesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().Named("foo"),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().Named("bar"),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecoratorViaProperty>().Named("baz"),
				Component.For<ArrayDepAsConstructor>().Named("InjectAll"),
				Component.For<ArrayDepAsConstructor>().Named("InjectFooOnly")
					.DependsOn(ServiceOverride.ForKey("services").Eq(new[] { "foo" })),
				Component.For<ArrayDepAsConstructor>().Named("InjectFooAndBarOnly")
					.DependsOn(ServiceOverride.ForKey("services").Eq(new[] { "foo", "bar" })),
				Component.For<ListDepAsConstructor>().Named("InjectAllList"),
				Component.For<ListDepAsConstructor>().Named("InjectFooOnlyList")
					.DependsOn(ServiceOverride.ForKey("services").Eq(new[] { "foo" })),
				Component.For<ListDepAsConstructor>().Named("InjectFooAndBarOnlyList")
					.DependsOn(ServiceOverride.ForKey("services").Eq(new[] { "foo", "bar" })));
		}
	}
}