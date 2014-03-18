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
	using System.Linq;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;

	using CastleTests.Components;

	using NUnit.Framework;

	public class ConventionRegistrationConfigureTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			// because some IEmptyService depend on collections
			Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
		}

		[Test]
		public void ConfigureIf_can_be_applied_multiple_times()
		{
			Container.Register(Classes.FromThisAssembly()
			                   	.BasedOn<IEmptyService>()
			                   	.ConfigureIf(r => r.Implementation.Name.EndsWith("A"), r => r.Named("A"))
			                   	.ConfigureIf(r => r.Implementation.Name.EndsWith("B"), r => r.Named("B")));

			var a = Container.Resolve<IEmptyService>("a");
			var b = Container.Resolve<IEmptyService>("b");

			Assert.IsInstanceOf<EmptyServiceA>(a);
			Assert.IsInstanceOf<EmptyServiceB>(b);
		}

		[Test]
		public void ConfigureIf_configures_all_matching_components()
		{
			Container.Register(Classes.FromThisAssembly()
			                   	.BasedOn<IEmptyService>()
			                   	.ConfigureIf(r => char.IsUpper(r.Implementation.Name.Last()), r => r.Named(r.Implementation.Name.Last().ToString())));

			var a = Container.Resolve<IEmptyService>("a");
			var b = Container.Resolve<IEmptyService>("b");

			Assert.IsInstanceOf<EmptyServiceA>(a);
			Assert.IsInstanceOf<EmptyServiceB>(b);
		}

		[Test]
		public void ConfigureIf_configures_matching_components()
		{
			Container.Register(Classes.FromThisAssembly()
			                   	.BasedOn<IEmptyService>()
			                   	.ConfigureIf(r => r.Implementation.Name.EndsWith("A"), r => r.Named("A")));

			var a = Container.Resolve<IEmptyService>("a");

			Assert.IsInstanceOf<EmptyServiceA>(a);
		}

		[Test]
		public void ConfigureIf_configures_matching_components_and_alternative_configuration_configures_the_rest()
		{
			var number = 0;
			Container.Register(Classes.FromThisAssembly()
			                   	.BasedOn<IEmptyService>()
			                   	.WithService.Base()
			                   	.ConfigureIf(r => r.Implementation.Name.EndsWith("A"), r => r.Named("A"), r => r.Named((++number).ToString())));

			var a = Container.Resolve<IEmptyService>("a");
			Assert.IsInstanceOf<EmptyServiceA>(a);

			Container.Resolve<IEmptyService>("1");
			Container.Resolve<IEmptyService>("2");
		}
	}
}