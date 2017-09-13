// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Extensions
{
	using System;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Extensions;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationExtensionsTestCase
	{
		private IWindsorContainer misconfiguredContainer = null;
		private IWindsorContainer correctlyConfiguredContainer = null;

		[SetUp]
		public void SetUp()
		{
			misconfiguredContainer = new WindsorContainer();
			misconfiguredContainer.Register(Component.For<IConfigurationExtensionInterface>().ImplementedBy<ConfigurationExtensionClass>());
			//misconfiguredContainer.Register(Component.For<IChildConfigurationExtensionInterface>().ImplementedBy<ChildConfigurationExtensionClass>()); <- whoops!

			correctlyConfiguredContainer = new WindsorContainer();
			correctlyConfiguredContainer.Register(Component.For<IConfigurationExtensionInterface>().ImplementedBy<ConfigurationExtensionClass>());
			correctlyConfiguredContainer.Register(Component.For<IChildConfigurationExtensionInterface>().ImplementedBy<ChildConfigurationExtensionClass>());
		}

		[Test]
		public void ValidateConfiguration_should_detect_when_components_are_not_resolvable()
		{
			var message = string.Empty;
			var result = misconfiguredContainer.ValidateConfiguration(out message);
			Assert.That(result, Is.False);
			Console.WriteLine(message);
		}

		[Test]
		public void ValidateConfiguration_should_pass_when_all_components_are_resolvable()
		{
			var message = string.Empty;
			var result = correctlyConfiguredContainer.ValidateConfiguration(out message);
			Assert.That(result, Is.True);
		}

		public interface IConfigurationExtensionInterface
		{
		}

		public class ConfigurationExtensionClass : IConfigurationExtensionInterface
		{
			private readonly IChildConfigurationExtensionInterface child;

			public ConfigurationExtensionClass(IChildConfigurationExtensionInterface child)
			{
				this.child = child;
			}
		}

		public interface IChildConfigurationExtensionInterface
		{
		}

		public class ChildConfigurationExtensionClass : IChildConfigurationExtensionInterface
		{
		}


	}
}