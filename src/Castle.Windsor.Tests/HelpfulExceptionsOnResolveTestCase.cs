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
	using System;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class HelpfulExceptionsOnResolveTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Resolving_by_name_not_found_prints_helpful_message_many_other_options_present()
		{
			Container.Register(Component.For<A>(),
			                   Component.For<A>().Named("something"));

			var exception =
				Assert.Throws<ComponentNotFoundException>(() =>
				                                          Container.Resolve<A>("Stefan-Mucha"));

			var expected =
				string.Format(
					"Requested component named 'Stefan-Mucha' was not found in the container. Did you forget to register it?{0}" +
					"There are 2 other components supporting requested service '{1}'. Were you looking for any of them?",
					Environment.NewLine, typeof(A).FullName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		public void Resolving_by_name_not_found_prints_helpful_message_one_other_option_present()
		{
			Container.Register(Component.For<A>());

			var exception =
				Assert.Throws<ComponentNotFoundException>(() =>
				                                          Container.Resolve<A>("Stefan-Mucha"));

			var expected =
				string.Format(
					"Requested component named 'Stefan-Mucha' was not found in the container. Did you forget to register it?{0}" +
					"There is one other component supporting requested service '{1}'. Is it what you were looking for?",
					Environment.NewLine, typeof(A).FullName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		public void Resolving_by_name_not_found_prints_helpful_message_zero_other_options_present()
		{
			var exception =
				Assert.Throws<ComponentNotFoundException>(() =>
				                                          Container.Resolve<A>("Stefan-Mucha"));

			var expected =
				string.Format(
					"Requested component named 'Stefan-Mucha' was not found in the container. Did you forget to register it?{0}" +
					"There are no components supporting requested service '{1}'. You need to register components in order to be able to use them.",
					Environment.NewLine, typeof(A).FullName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		public void When_property_setter_throws_at_resolution_time_exception_suggests_disabling_setting_the_property()
		{
			Container.Register(
				Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
				Component.For<PropertySetterThrows>());

			var exception = Assert.Throws<ComponentActivatorException>(() => Container.Resolve<PropertySetterThrows>());

			var message = string.Format("Error setting property PropertySetterThrows.CommonService in component {1}. See inner exception for more information.{0}" +
			                            "If you don't want Windsor to set this property you can do it by either decorating it with {2} or via registration API.{0}" +
			                            "Alternatively consider making the setter non-public.",
			                            Environment.NewLine, typeof(PropertySetterThrows), typeof(DoNotWireAttribute));

			Assert.AreEqual(message, exception.Message);
		}
	}
}