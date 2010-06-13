// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using System;

	using Castle.Core.Configuration;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class UnsatisfiedDependenciesTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void UnsatisfiedService()
		{
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("key"));

			Assert.Throws(typeof(HandlerException), () =>
			{
				object instance = kernel["key"];
			});
		}

		[Test]
		public void UnsatisfiedConfigValues()
		{
			MutableConfiguration config = new MutableConfiguration("component");

			MutableConfiguration parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("name", "hammett"));

			kernel.ConfigurationStore.AddComponentConfiguration("customer", config);

			kernel.Register(Component.For(typeof(CustomerImpl2)).Named("key"));

			var exception =
				Assert.Throws(typeof(HandlerException), () =>
				{
					object instance = kernel["key"];
				});
			var expectedMessage =
				string.Format(
					"Can't create component 'key' as it has dependencies to be satisfied. {0}key is waiting for the following dependencies: {0}{0}" +
					"Keys (components with specific keys){0}- name which was not registered. {0}- address which was not registered. {0}" +
					"- age which was not registered. {0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void UnsatisfiedOverride()
		{
			MutableConfiguration config = new MutableConfiguration("component");

			MutableConfiguration parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${common2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common1"));
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("key"));
			var exception =
				Assert.Throws(typeof(HandlerException), () =>
				{
					object instance = kernel["key"];
				});
			var expectedMessage =
				string.Format(
					"Can't create component 'key' as it has dependencies to be satisfied. {0}key is waiting for the following dependencies: {0}{0}Keys (components with specific keys){0}- common2 which was not registered. {0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void OverrideIsForcedDependency()
		{
			MutableConfiguration config = new MutableConfiguration("component");

			MutableConfiguration parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${common2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common1"));
			kernel.Register(Component.For(typeof(CommonServiceUser3)).Named("key"));
			var exception =
				Assert.Throws(typeof(HandlerException), () =>
				{
					object instance = kernel["key"];
				});
			var expectedMessage =
				string.Format(
					"Can't create component 'key' as it has dependencies to be satisfied. {0}key is waiting for the following dependencies: {0}{0}Keys (components with specific keys){0}- common2 which was not registered. {0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage,exception.Message);
		}

		[Test]
		public void SatisfiedOverride()
		{
			MutableConfiguration config = new MutableConfiguration("component");

			MutableConfiguration parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${common2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common1"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("common2"));
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("key"));
			CommonServiceUser instance = (CommonServiceUser) kernel["key"];

			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.CommonService);
			Assert.AreEqual("CommonImpl2", instance.CommonService.GetType().Name);
		}

		[Test]
		public void SatisfiedOverrideRecursive()
		{
			MutableConfiguration config1 = new MutableConfiguration("component");
			MutableConfiguration parameters1 = new MutableConfiguration("parameters");
			config1.Children.Add(parameters1);
			parameters1.Children.Add(new MutableConfiguration("inner", "${repository2}"));
			kernel.ConfigurationStore.AddComponentConfiguration("repository1", config1);
			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(Repository1)).Named("repository1"));

			MutableConfiguration config2 = new MutableConfiguration("component");
			MutableConfiguration parameters2 = new MutableConfiguration("parameters");
			config2.Children.Add(parameters2);
			parameters2.Children.Add(new MutableConfiguration("inner", "${repository3}"));
			kernel.ConfigurationStore.AddComponentConfiguration("repository2", config2);
			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(Repository2)).Named("repository2"));

			MutableConfiguration config3 = new MutableConfiguration("component");
			MutableConfiguration parameters3 = new MutableConfiguration("parameters");
			config3.Children.Add(parameters3);
			parameters3.Children.Add(new MutableConfiguration("inner", "${decoratedRepository}"));
			kernel.ConfigurationStore.AddComponentConfiguration("repository3", config3);
			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(Repository3)).Named("repository3"));

			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(DecoratedRepository)).Named("decoratedRepository"));

			IRepository instance = (Repository1) kernel[typeof(IRepository)];

			Assert.IsNotNull(instance);
			Assert.IsInstanceOf(typeof(Repository1), instance);
			Assert.IsInstanceOf(typeof(Repository2), ((Repository1) instance).InnerRepository);
			Assert.IsInstanceOf(typeof(Repository3),
			                        ((Repository2) (((Repository1) instance).InnerRepository)).InnerRepository);
			Assert.IsInstanceOf(typeof(DecoratedRepository),
			                        ((Repository3) (((Repository2) (((Repository1) instance).InnerRepository)).InnerRepository)).
			                        	InnerRepository);
		}
	}
}
