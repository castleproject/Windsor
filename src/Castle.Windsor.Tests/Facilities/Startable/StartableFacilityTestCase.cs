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

namespace Castle.Windsor.Tests.Facilities.Startable
{
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.Startable.Components;

	using NUnit.Framework;

	[TestFixture]
	public class StartableFacilityTestCase
	{
		[SetUp]
		public void SetUp()
		{
			startableCreatedBeforeResolved = false;
		}

		private bool startableCreatedBeforeResolved;

		private void OnStartableComponentStarted(ComponentModel mode, object instance)
		{
			var startable = instance as StartableComponent;

			Assert.IsNotNull(startable);
			Assert.IsTrue(startable.Started);
			Assert.IsFalse(startable.Stopped);

			startableCreatedBeforeResolved = true;
		}

		private void OnNoInterfaceStartableComponentStarted(ComponentModel mode, object instance)
		{
			var startable = instance as NoInterfaceStartableComponent;

			Assert.IsNotNull(startable);
			Assert.IsTrue(startable.Started);
			Assert.IsFalse(startable.Stopped);

			startableCreatedBeforeResolved = true;
		}

		[Test]
		public void Starts_component_without_start_method()
		{
			ClassWithInstanceCount.InstancesCount = 0;
			IKernel kernel = new DefaultKernel();
			kernel.AddFacility<StartableFacility>(f => f.DeferredTryStart());
			kernel.Register(Component.For<ClassWithInstanceCount>().Start());
			Assert.AreEqual(1, ClassWithInstanceCount.InstancesCount);
		}

		[Test]
		public void Starts_component_without_start_method_AllTypes()
		{
			ClassWithInstanceCount.InstancesCount = 0;
			IKernel kernel = new DefaultKernel();
			kernel.AddFacility<StartableFacility>(f => f.DeferredTryStart());
			kernel.Register(AllTypes.FromThisAssembly()
			                	.Where(t => t == typeof(ClassWithInstanceCount))
			                	.Configure(c => c.Start()));
			Assert.AreEqual(1, ClassWithInstanceCount.InstancesCount);
		}

		[Test]
		public void TestComponentWithNoInterface()
		{
			IKernel kernel = new DefaultKernel();
			kernel.ComponentCreated += OnNoInterfaceStartableComponentStarted;

			var compNode = new MutableConfiguration("component");
			compNode.Attributes["id"] = "b";
			compNode.Attributes["startable"] = "true";
			compNode.Attributes["startMethod"] = "Start";
			compNode.Attributes["stopMethod"] = "Stop";

			kernel.ConfigurationStore.AddComponentConfiguration("b", compNode);

			kernel.AddFacility("startable", new StartableFacility());
			kernel.Register(Component.For(typeof(NoInterfaceStartableComponent)).Named("b"));

			Assert.IsTrue(startableCreatedBeforeResolved, "Component was not properly started");

			var component = kernel.Resolve<NoInterfaceStartableComponent>("b");

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}

		[Test]
		public void TestInterfaceBasedStartable()
		{
			IKernel kernel = new DefaultKernel();
			kernel.ComponentCreated += OnStartableComponentStarted;

			kernel.AddFacility("startable", new StartableFacility());

			kernel.Register(Component.For(typeof(StartableComponent)).Named("a"));

			Assert.IsTrue(startableCreatedBeforeResolved, "Component was not properly started");

			var component = kernel.Resolve<StartableComponent>("a");

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}

		/// <summary>
		/// This test has one startable component dependent on another, and both are dependent
		/// on a third generic component - all are singletons. We need to make sure we only get
		/// one instance of each component created.
		/// </summary>
		[Test]
		public void TestStartableChainWithGenerics()
		{
			IKernel kernel = new DefaultKernel();

			kernel.AddFacility("startable", new StartableFacility());

			// Add parent. This has a dependency so won't be started yet.
			kernel.Register(Component.For(typeof(StartableChainParent)).Named("chainparent"));

			Assert.AreEqual(0, StartableChainDependency.startcount);
			Assert.AreEqual(0, StartableChainDependency.createcount);

			// Add generic dependency. This is not startable so won't get created. 
			kernel.Register(Component.For(typeof(StartableChainGeneric<>)).Named("chaingeneric"));

			Assert.AreEqual(0, StartableChainDependency.startcount);
			Assert.AreEqual(0, StartableChainDependency.createcount);

			// Add dependency. This will satisfy the dependency so everything will start.
			kernel.Register(Component.For(typeof(StartableChainDependency)).Named("chaindependency"));

			Assert.AreEqual(1, StartableChainParent.startcount);
			Assert.AreEqual(1, StartableChainParent.createcount);
			Assert.AreEqual(1, StartableChainDependency.startcount);
			Assert.AreEqual(1, StartableChainDependency.createcount);
			Assert.AreEqual(1, StartableChainGeneric<string>.createcount);
		}

		[Test]
		public void TestStartableCustomDependencies()
		{
			IKernel kernel = new DefaultKernel();
			kernel.ComponentCreated += OnStartableComponentStarted;

			kernel.AddFacility("startable", new StartableFacility());

			kernel.Register(
				Component.For<StartableComponentCustomDependencies>()
					.Named("a")
					.DependsOn(Property.ForKey("config").Eq(1))
				);
			Assert.IsTrue(startableCreatedBeforeResolved, "Component was not properly started");

			var component = kernel.Resolve<StartableComponentCustomDependencies>("a");

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}

		[Test]
		public void TestStartableWithRegisteredCustomDependencies()
		{
			IKernel kernel = new DefaultKernel();
			kernel.ComponentCreated += OnStartableComponentStarted;

			kernel.AddFacility("startable", new StartableFacility());

			var dependencies = new Dictionary<string, object> { { "config", 1 } };
			kernel.Register(Component.For(typeof(StartableComponentCustomDependencies)).Named("a"));
			kernel.RegisterCustomDependencies(typeof(StartableComponentCustomDependencies), dependencies);

			Assert.IsTrue(startableCreatedBeforeResolved, "Component was not properly started");

			var component = kernel.Resolve<StartableComponentCustomDependencies>("a");

			Assert.IsNotNull(component);
			Assert.IsTrue(component.Started);
			Assert.IsFalse(component.Stopped);

			kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Stopped);
		}
	}

	public class ClassWithInstanceCount
	{
		public static int InstancesCount;

		public ClassWithInstanceCount()
		{
			InstancesCount++;
		}
	}
}