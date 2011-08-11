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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class PropertyDependenciesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_opt_out_of_setting_properties()
		{
			Container.Register(
				Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
				Component.For<CommonServiceUser2>()
					.Properties(p => false));

			var item = Container.Resolve<CommonServiceUser2>();
			Assert.IsNull(item.CommonService);
		}

		[Test]
		public void Can_opt_out_of_setting_properties_open_generic()
		{
			Container.Register(Component.For(typeof(GenericImpl2<>))
			                   	.DependsOn(Dependency.OnValue(typeof(int), 5))
			                   	.Properties(p => false));

			var item = Container.Resolve<GenericImpl2<A>>();
			Assert.AreEqual(0, item.Value);
		}

		[Test]
		[Ignore("This should be probably handled by a component model construction contributor")]
		public void Can_require_setting_properties()
		{
			Container.Register(
				Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
				Component.For<CommonServiceUser2>());

			Assert.Throws<ComponentResolutionException>(() => Container.Resolve<CommonServiceUser2>());
		}

		[Test]
		[Ignore("This should be probably handled by a component model construction contributor")]
		public void Can_require_setting_properties_open_generic()
		{
			Container.Register(Component.For(typeof(GenericImpl2<>)));

			Assert.Throws<ComponentResolutionException>(() => Container.Resolve<GenericImpl2<A>>());
		}
	}
}