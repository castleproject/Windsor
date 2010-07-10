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
	using System.Collections.ObjectModel;

	using Castle.Windsor.Tests.Components;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class OpenGenericsTestCase
	{
		[SetUp]
		public void Setup()
		{
			container = new WindsorContainer();
		}

		private WindsorContainer container;

		[Test]
		public void ExtendedProperties_incl_ProxyOptions_are_honored_for_open_generic_types()
		{
			container.Register(
				Component.For(typeof(Collection<>))
					.Proxy.AdditionalInterfaces(typeof(ISimpleService)));

			var proxy = container.Resolve<Collection<int>>();

			Assert.IsInstanceOf<ISimpleService>(proxy);
		}

		[Test]
		public void ResolveAll_properly_skips_open_generic_service_with_generic_constraints_that_dont_match()
		{
			container.Register(
				Component.For(typeof(IHasGenericConstraints<,>))
					.ImplementedBy(typeof(HasGenericConstraintsImpl<,>)));

			var invalid = container.ResolveAll<IHasGenericConstraints<EmptySub1, EmptyClass>>();

			Assert.AreEqual(0, invalid.Length);
		}

		[Test]
		public void ResolveAll_returns_matching_open_generic_service_with_generic_constraints()
		{
			container.Register(
				Component.For(typeof(IHasGenericConstraints<,>))
					.ImplementedBy(typeof(HasGenericConstraintsImpl<,>)));

			var valid = container.ResolveAll<IHasGenericConstraints<EmptySub2WithMarkerInterface, EmptyClass>>();

			Assert.AreEqual(1, valid.Length);
		}
	}
}