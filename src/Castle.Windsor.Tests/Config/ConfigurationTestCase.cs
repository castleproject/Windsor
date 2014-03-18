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

namespace Castle.MicroKernel.Tests.Configuration
{
	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Resource;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Configuration.Components;
	using Castle.Windsor;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests.Components;

	using CastleTests;
	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationTestCase : AbstractContainerTestCase
	{
#if !SILVERLIGHT
		[Test]
		[Bug("IOC-155")]
		public void Type_not_implementing_service_should_throw()
		{
			var exception = Assert.Throws<ComponentRegistrationException>(() =>
			                                                              Container.Install(Configuration.FromXml(
			                                                              	new StaticContentResource(
			                                                              		@"<castle>
<components>
    <component
        service=""EmptyServiceA""
        type=""IEmptyService""/>
</components>
</castle>"))));

			var expected = string.Format("Could not set up component '{0}'. Type '{1}' does not implement service '{2}'",
			                             typeof(IEmptyService).FullName,
			                             typeof(IEmptyService).AssemblyQualifiedName,
			                             typeof(EmptyServiceA).AssemblyQualifiedName);

			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		[Bug("IOC-197")]
		public void DictionaryAsParameterInXml()
		{
			Container.Install(Configuration.FromXml(
				new StaticContentResource(
					string.Format(
						@"<castle>
<components>
	<component lifestyle=""singleton""
		id=""Id.MyClass""
		type=""{0}"">
		<parameters>
			<DictionaryProperty>${{Id.dictionary}}</DictionaryProperty>
		</parameters>
	</component>

	<component id=""Id.dictionary"" lifestyle=""singleton""
						 service=""System.Collections.IDictionary, mscorlib""
						 type=""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]]""
						 >
		<parameters>
			<dictionary>
				<dictionary>
					<entry key=""string.key.1"">string value 1</entry>
					<entry key=""string.key.2"">string value 2</entry>
				</dictionary>
			</dictionary>
		</parameters>
	</component>
</components>
</castle>",
						typeof(HasDictionaryDependency).AssemblyQualifiedName))));

			var myInstance = Container.Resolve<HasDictionaryDependency>();
			Assert.AreEqual(2, myInstance.DictionaryProperty.Count);
		}

		[Test]
		[Bug("IOC-73")]
		public void ShouldNotThrowCircularDependencyException()
		{
			var config =
				@"
<configuration>
    <facilities>
    </facilities>
    <components>
        <component id='MyClass'
            service='IEmptyService'
            type='EmptyServiceA'/>
        <component id='Proxy'
            service='IEmptyService'
            type='EmptyServiceDecorator'>
            <parameters>
                <other>${MyClass}</other>
            </parameters>
        </component>
        <component id='ClassUser'
            type='UsesIEmptyService'>
            <parameters>
                <emptyService>${Proxy}</emptyService>
            </parameters>
        </component>
    </components>
</configuration>";

			Container.Install(Configuration.FromXml(new StaticContentResource(config)));
			var user = Container.Resolve<UsesIEmptyService>();
			Assert.NotNull(user.EmptyService);
		}
#endif

		[Test]
		[Bug("IOC-142")]
		public void Can_satisfy_nullable_ctor_dependency()
		{
			var container = new WindsorContainer();
			var configuration = new MutableConfiguration("parameters");
			configuration.CreateChild("foo", "5");
			container.Register(Component.For<HasNullableDoubleConstructor>().Configuration(configuration));

			container.Resolve<HasNullableDoubleConstructor>();
		}

		[Test]
		[Bug("IOC-142")]
		public void Can_satisfy_nullable_property_dependency()
		{
			var container = new WindsorContainer();
			var configuration = new MutableConfiguration("parameters");
			configuration.CreateChild("SomeVal", "5");
			container.Register(Component.For<HasNullableIntProperty>().Configuration(configuration));

			var s = container.Resolve<HasNullableIntProperty>();
			Assert.IsNotNull(s.SomeVal);
		}

		[Test]
		public void ComplexConfigurationParameter()
		{
			var key = "key";
			var value1 = "value1";
			var value2 = "value2";

			var confignode = new MutableConfiguration(key);

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration complexParam = new MutableConfiguration("complexparam");
			parameters.Children.Add(complexParam);

			IConfiguration complexNode = new MutableConfiguration("complexparametertype");
			complexParam.Children.Add(complexNode);

			complexNode.Children.Add(new MutableConfiguration("mandatoryvalue", value1));
			complexNode.Children.Add(new MutableConfiguration("optionalvalue", value2));

			Kernel.ConfigurationStore.AddComponentConfiguration(key, confignode);
			Kernel.Register(Component.For(typeof(ClassWithComplexParameter)).Named(key));

			var instance = Kernel.Resolve<ClassWithComplexParameter>(key);

			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.ComplexParam);
			Assert.AreEqual(value1, instance.ComplexParam.MandatoryValue);
			Assert.AreEqual(value2, instance.ComplexParam.OptionalValue);
		}

		[Test]
		public void ConstructorWithArrayParameter()
		{
			var confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration hosts = new MutableConfiguration("hosts");
			parameters.Children.Add(hosts);
			IConfiguration array = new MutableConfiguration("array");
			hosts.Children.Add(array);
			array.Children.Add(new MutableConfiguration("item", "castle"));
			array.Children.Add(new MutableConfiguration("item", "uol"));
			array.Children.Add(new MutableConfiguration("item", "folha"));

			Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

			Kernel.Register(Component.For(typeof(ClassWithConstructors)).Named("key"));

			var instance = Kernel.Resolve<ClassWithConstructors>("key");
			Assert.IsNotNull(instance);
			Assert.IsNull(instance.Host);
			Assert.AreEqual("castle", instance.Hosts[0]);
			Assert.AreEqual("uol", instance.Hosts[1]);
			Assert.AreEqual("folha", instance.Hosts[2]);
		}

		[Test]
		public void ConstructorWithArrayParameterAndCustomType()
		{
			var confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration services = new MutableConfiguration("services");
			parameters.Children.Add(services);
			var array = new MutableConfiguration("array");
			services.Children.Add(array);

			array.Children.Add(new MutableConfiguration("item", "${commonservice1}"));
			array.Children.Add(new MutableConfiguration("item", "${commonservice2}"));

			Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

			Kernel.Register(Component.For<ClassWithArrayConstructor>().Named("key"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("commonservice1"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("commonservice2"));

			var instance = Kernel.Resolve<ClassWithArrayConstructor>("key");
			Assert.IsNotNull(instance.Services);
			Assert.AreEqual(2, instance.Services.Length);
			Assert.AreEqual("CommonImpl1", instance.Services[0].GetType().Name);
			Assert.AreEqual("CommonImpl2", instance.Services[1].GetType().Name);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif

		[Test]
		public void ConstructorWithListParameterAndCustomType()
		{
			var confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			IConfiguration services = new MutableConfiguration("services");
			parameters.Children.Add(services);
			var list = new MutableConfiguration("list");
			services.Children.Add(list);
			list.Attributes.Add("type", "ICommon");

			list.Children.Add(new MutableConfiguration("item", "${commonservice1}"));
			list.Children.Add(new MutableConfiguration("item", "${commonservice2}"));

			Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);
			Kernel.Register(Component.For(typeof(ClassWithListConstructor)).Named("key"));

			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));

			var instance = Kernel.Resolve<ClassWithListConstructor>("key");
			Assert.IsNotNull(instance.Services);
			Assert.AreEqual(2, instance.Services.Count);
			Assert.AreEqual("CommonImpl1", instance.Services[0].GetType().Name);
			Assert.AreEqual("CommonImpl2", instance.Services[1].GetType().Name);
		}

		[Test]
		public void ConstructorWithStringParameters()
		{
			var confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("host", "castleproject.org"));

			Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

			Kernel.Register(Component.For<ClassWithConstructors>().Named("key"));

			var instance = Kernel.Resolve<ClassWithConstructors>("key");
			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.Host);
			Assert.AreEqual("castleproject.org", instance.Host);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif

		[Test]
		public void CustomLifestyleManager()
		{
			var key = "key";

			var confignode = new MutableConfiguration(key);
			confignode.Attributes.Add("lifestyle", "custom");

			confignode.Attributes.Add("customLifestyleType", "CustomLifestyleManager");

			Kernel.ConfigurationStore.AddComponentConfiguration(key, confignode);
			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named(key));

			var instance = Kernel.Resolve<ICommon>(key);
			var handler = Kernel.GetHandler(key);

			Assert.IsNotNull(instance);
			Assert.AreEqual(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
			Assert.AreEqual(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);
		}

		[Test]
		public void ServiceOverride()
		{
			var confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("common", "${commonservice2}"));

			Kernel.ConfigurationStore.AddComponentConfiguration("commonserviceuser", confignode);

			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));
			Kernel.Register(Component.For(typeof(CommonServiceUser)).Named("commonserviceuser"));

			var instance = Kernel.Resolve<CommonServiceUser>("commonserviceuser");

			Assert.IsNotNull(instance);
			Assert.AreEqual(typeof(CommonImpl2), instance.CommonService.GetType());
		}

		[Test]
		public void ServiceOverrideUsingProperties()
		{
			var confignode = new MutableConfiguration("key");

			IConfiguration parameters = new MutableConfiguration("parameters");
			confignode.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("CommonService", "${commonservice2}"));

			Kernel.ConfigurationStore.AddComponentConfiguration("commonserviceuser", confignode);

			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("commonservice1"));
			Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl2)).Named("commonservice2"));

			Kernel.Register(Component.For(typeof(CommonServiceUser2)).Named("commonserviceuser"));

			var instance = Kernel.Resolve<CommonServiceUser2>("commonserviceuser");

			Assert.IsNotNull(instance);
			Assert.AreEqual(typeof(CommonImpl2), instance.CommonService.GetType());
		}

        [Test]
        public void Works_when_registered_as_a_dependency_with_conventions()
        {
            // Arrange
            Container.Install(Configuration.FromXmlFile("config\\simple.config"));

            var kernelPre = Kernel.GetHandlers(typeof(IConfig));

            // Act
            Container.Register(Classes.FromThisAssembly().Pick().WithServiceFirstInterface());
            var kernelPost = Kernel.GetHandlers(typeof(IConfig));

            var configDependency = Container.Resolve<IClassWithConfigDependency>();

            // Assert
            Assert.AreEqual(configDependency.GetName(), "value");
            Assert.AreEqual(configDependency.GetServerIp("Database"), "3.24.23.33"); // this is where it fails i.e. config.Servers is null
        }
	}
}