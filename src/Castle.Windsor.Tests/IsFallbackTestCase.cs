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
	using System.Reflection;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	public class IsFallbackTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_make_a_component_fallback_via_AllTypes_1()
		{
			Container.Register(
				Classes.From(typeof(EmptyServiceA), typeof(EmptyServiceB))
					.BasedOn<IEmptyService>()
					.WithService.Base()
					.ConfigureFor<EmptyServiceB>(c => c.IsFallback()));
			var obj = Container.Resolve<IEmptyService>();

			Assert.IsNotInstanceOf<EmptyServiceB>(obj);
		}

		[Test]
		public void Can_make_a_component_fallback_via_AllTypes_2()
		{
			Container.Register(
				Classes.From(typeof(EmptyServiceA), typeof(EmptyServiceB))
					.BasedOn<IEmptyService>()
					.WithService.Base()
					.ConfigureFor<EmptyServiceA>(c => c.IsFallback()));
			var obj = Container.Resolve<IEmptyService>();

			Assert.IsNotInstanceOf<EmptyServiceA>(obj);
		}

		[Test]
		public void Can_make_first_component_default_with_filter()
		{
			Container.Register(Component.For<IEmptyService, EmptyServiceA, object>().ImplementedBy<EmptyServiceA>().IsFallback(t => t.GetTypeInfo().IsInterface),
			                   Component.For<IEmptyService, EmptyServiceB, object>().ImplementedBy<EmptyServiceB>());

			var obj = Container.Resolve<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceB>(obj);

			var obj2 = Container.Resolve<object>();
			Assert.IsInstanceOf<EmptyServiceA>(obj2);
		}

		[Test]
		public void Can_make_first_component_fallback()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().IsFallback(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

			var obj = Container.Resolve<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceB>(obj);
		}

		[Test]
		public void Does_affect_order_when_using_ResolveAll()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().IsFallback(t => t.GetTypeInfo().IsInterface),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

			var obj = Container.ResolveAll<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceB>(obj[0]);
			Assert.IsInstanceOf<EmptyServiceA>(obj[1]);
		}

		[Test]
		public void Later_fallback_does_not_override_earlier_one()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().IsFallback(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsFallback());

			var obj = Container.Resolve<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceA>(obj);
		}
	}
}