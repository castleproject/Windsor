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

		private IKernel kernel;

		[Test]
		public void OverrideIsForcedDependency()
		{
			var config = new MutableConfiguration("component");

			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${common2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common1"));
			kernel.Register(Component.For(typeof(CommonServiceUser3)).Named("key"));
			var exception =
				Assert.Throws(typeof(HandlerException), () => kernel.Resolve("key", new Arguments()));
			var expectedMessage =
				string.Format(
					"Can't create component 'key' as it has dependencies to be satisfied. {0}key is waiting for the following dependencies: {0}{0}Keys (components with specific keys){0}- common2 which was not registered. {0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void SatisfiedOverride()
		{
			var config = new MutableConfiguration("component");

			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${common2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common1"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("common2"));
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("key"));
			var instance = (CommonServiceUser)kernel.Resolve("key", new Arguments());

			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.CommonService);
			Assert.AreEqual("CommonImpl2", instance.CommonService.GetType().Name);
		}

		[Test]
		public void SatisfiedOverrideRecursive()
		{
			var config1 = new MutableConfiguration("component");
			var parameters1 = new MutableConfiguration("parameters");
			config1.Children.Add(parameters1);
			parameters1.Children.Add(new MutableConfiguration("inner", "${repository2}"));
			kernel.ConfigurationStore.AddComponentConfiguration("repository1", config1);
			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(Repository1)).Named("repository1"));

			var config2 = new MutableConfiguration("component");
			var parameters2 = new MutableConfiguration("parameters");
			config2.Children.Add(parameters2);
			parameters2.Children.Add(new MutableConfiguration("inner", "${repository3}"));
			kernel.ConfigurationStore.AddComponentConfiguration("repository2", config2);
			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(Repository2)).Named("repository2"));

			var config3 = new MutableConfiguration("component");
			var parameters3 = new MutableConfiguration("parameters");
			config3.Children.Add(parameters3);
			parameters3.Children.Add(new MutableConfiguration("inner", "${decoratedRepository}"));
			kernel.ConfigurationStore.AddComponentConfiguration("repository3", config3);
			kernel.Register(Component.For(typeof(IRepository)).ImplementedBy(typeof(Repository3)).Named("repository3"));

			kernel.Register(
				Component.For(typeof(IRepository)).ImplementedBy(typeof(DecoratedRepository)).Named("decoratedRepository"));

			var instance = kernel.Resolve<IRepository>();

			Assert.IsNotNull(instance);
			Assert.IsInstanceOf<Repository1>(instance);
			Assert.IsInstanceOf<Repository2>(((Repository1)instance).InnerRepository);
			Assert.IsInstanceOf<Repository3>(
				((Repository2)(((Repository1)instance).InnerRepository)).InnerRepository);
			Assert.IsInstanceOf<DecoratedRepository>(
				((Repository3)(((Repository2)(((Repository1)instance).InnerRepository)).InnerRepository)).
					InnerRepository);
		}

		[Test]
		public void UnsatisfiedConfigValues()
		{
			var config = new MutableConfiguration("component");

			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("name", "hammett"));

			kernel.ConfigurationStore.AddComponentConfiguration("customer", config);

			kernel.Register(Component.For(typeof(CustomerImpl2)).Named("key"));

			var exception =
				Assert.Throws(typeof(HandlerException), () => { var instance = kernel.Resolve("key", new Arguments()); });
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
			var config = new MutableConfiguration("component");

			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${common2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common1"));
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("key"));
			var exception =
				Assert.Throws(typeof(HandlerException), () => { var instance = kernel.Resolve("key", new Arguments()); });
			var expectedMessage =
				string.Format(
					"Can't create component 'key' as it has dependencies to be satisfied. {0}key is waiting for the following dependencies: {0}{0}Keys (components with specific keys){0}- common2 which was not registered. {0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void UnsatisfiedService()
		{
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("key"));

			Assert.Throws(typeof(HandlerException), () => { var instance = kernel.Resolve("key", new Arguments()); });
		}
	}
}