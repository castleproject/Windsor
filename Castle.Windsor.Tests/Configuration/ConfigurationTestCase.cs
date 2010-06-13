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

namespace Castle.MicroKernel.Tests.Configuration
{
    using System;

    using Castle.Core.Configuration;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Configuration.Components;
	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationTestCase
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
		[ExpectedException(typeof(DependencyResolverException))]
		public void ConstructorWithUnsatisfiedParameters()
		{
			kernel.Register(Component.For(typeof(ClassWithConstructors)).Named("key"));
			object res = kernel["key"];
		}

		[Test]
		public void ConstructorWithStringParameters()
		{
			MutableConfiguration confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
            confignode.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("host", "castleproject.org"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

			kernel.Register(Component.For(typeof(ClassWithConstructors)).Named("key"));

			ClassWithConstructors instance = (ClassWithConstructors) kernel["key"];
			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.Host);
			Assert.AreEqual("castleproject.org", instance.Host);
		}

		[Test]
		public void ServiceOverride()
		{
			MutableConfiguration confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${commonservice2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("commonserviceuser", confignode);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));
			kernel.Register(Component.For(typeof(CommonServiceUser)).Named("commonserviceuser"));

			CommonServiceUser instance = (CommonServiceUser) kernel["commonserviceuser"];

			Assert.IsNotNull(instance);
			Assert.AreEqual(typeof(CommonImpl2), instance.CommonService.GetType());
		}

		[Test]
		public void ServiceOverrideUsingProperties()
		{
			MutableConfiguration confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("CommonService", "${commonservice2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("commonserviceuser", confignode);

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));

			kernel.Register(Component.For(typeof(CommonServiceUser2)).Named("commonserviceuser"));

			CommonServiceUser2 instance = (CommonServiceUser2) kernel["commonserviceuser"];

			Assert.IsNotNull(instance);
			Assert.AreEqual(typeof(CommonImpl2), instance.CommonService.GetType());
		}

		[Test]
		public void ConstructorWithArrayParameter()
		{
			MutableConfiguration confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration hosts = new MutableConfiguration("hosts");
			parameters.Children.Add(hosts);
			IConfiguration array = new MutableConfiguration("array");
			hosts.Children.Add(array);
			array.Children.Add(new MutableConfiguration("item", "castle"));
			array.Children.Add(new MutableConfiguration("item", "uol"));
			array.Children.Add(new MutableConfiguration("item", "folha"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

			kernel.Register(Component.For(typeof(ClassWithConstructors)).Named("key"));

			ClassWithConstructors instance = (ClassWithConstructors) kernel["key"];
			Assert.IsNotNull(instance);
			Assert.IsNull(instance.Host);
			Assert.AreEqual("castle", instance.Hosts[0]);
			Assert.AreEqual("uol", instance.Hosts[1]);
			Assert.AreEqual("folha", instance.Hosts[2]);
		}

		[Test]
		public void ConstructorWithListParameterAndCustomType()
		{
			MutableConfiguration confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration services = new MutableConfiguration("services");
			parameters.Children.Add(services);
			MutableConfiguration list = new MutableConfiguration("list");
			services.Children.Add(list);
			list.Attributes.Add("type", "Castle.MicroKernel.Tests.ClassComponents.ICommon, Castle.Windsor.Tests");

			list.Children.Add(new MutableConfiguration("item", "${commonservice1}"));
			list.Children.Add(new MutableConfiguration("item", "${commonservice2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);
			kernel.Register(Component.For(typeof(ClassWithListConstructor)).Named("key"));

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));

			ClassWithListConstructor instance = (ClassWithListConstructor) kernel["key"];
			Assert.IsNotNull(instance.Services);
			Assert.AreEqual(2, instance.Services.Count);
			Assert.AreEqual("CommonImpl1", instance.Services[0].GetType().Name);
			Assert.AreEqual("CommonImpl2", instance.Services[1].GetType().Name);
		}

		[Test]
		public void ConstructorWithArrayParameterAndCustomType()
		{
			MutableConfiguration confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration services = new MutableConfiguration("services");
			parameters.Children.Add(services);
			MutableConfiguration array = new MutableConfiguration("array");
			services.Children.Add(array);

			array.Children.Add(new MutableConfiguration("item", "${commonservice1}"));
			array.Children.Add(new MutableConfiguration("item", "${commonservice2}"));

			kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

			kernel.Register(Component.For(typeof(ClassWithArrayConstructor)).Named("key"));

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));

			ClassWithArrayConstructor instance = (ClassWithArrayConstructor) kernel["key"];
			Assert.IsNotNull(instance.Services);
			Assert.AreEqual(2, instance.Services.Length);
			Assert.AreEqual("CommonImpl1", instance.Services[0].GetType().Name);
			Assert.AreEqual("CommonImpl2", instance.Services[1].GetType().Name);
		}

		[Test]
		public void CustomLifestyleManager()
		{
			string key = "key";

			MutableConfiguration confignode = new MutableConfiguration(key);
			confignode.Attributes.Add("lifestyle", "custom");

			confignode.Attributes.Add("customLifestyleType",
									  "Castle.MicroKernel.Tests.ClassComponents.CustomLifestyleManager, Castle.Windsor.Tests");

			kernel.ConfigurationStore.AddComponentConfiguration(key, confignode);
			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named(key));

			ICommon instance = (ICommon) kernel[key];
			IHandler handler = kernel.GetHandler(key);

			Assert.IsNotNull(instance);
			Assert.AreEqual(Core.LifestyleType.Custom, handler.ComponentModel.LifestyleType);
			Assert.AreEqual(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);
		}

		[Test]
		public void ComplexConfigurationParameter()
		{
			string key = "key";
			string value1 = "value1";
			string value2 = "value2";

			MutableConfiguration confignode = new MutableConfiguration(key);

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration complexParam = new MutableConfiguration("complexparam");
			parameters.Children.Add(complexParam);

			IConfiguration complexNode = new MutableConfiguration("complexparametertype");
			complexParam.Children.Add(complexNode);

			complexNode.Children.Add(new MutableConfiguration("mandatoryvalue", value1));
			complexNode.Children.Add(new MutableConfiguration("optionalvalue", value2));


			kernel.ConfigurationStore.AddComponentConfiguration(key, confignode);
			kernel.Register(Component.For(typeof(ClassWithComplexParameter)).Named(key));

			ClassWithComplexParameter instance = (ClassWithComplexParameter) kernel[key];

			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.ComplexParam);
			Assert.AreEqual(value1, instance.ComplexParam.MandatoryValue);
			Assert.AreEqual(value2, instance.ComplexParam.OptionalValue);
		}
	}
}