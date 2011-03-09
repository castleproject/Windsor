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

namespace Castle.MicroKernel.Tests.Lifestyle
{
	using System;
	using System.Threading;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Lifestyle.Components;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	/// <summary>
	///   Summary description for LifestyleManagerTestCase.
	/// </summary>
	[TestFixture]
	public class LifestyleManagerTestCase
	{
		[SetUp]
		public void CreateContainer()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void DisposeContainer()
		{
			kernel.Dispose();
		}

		private IKernel kernel;

		private IComponent instance3;

		private void TestLifestyleAndSameness(Type componentType, LifestyleType lifestyle, bool overwrite, bool areSame)
		{
			var key = TestHandlersLifestyle(componentType, lifestyle, overwrite);
			TestSameness(key, areSame);
		}

		private void TestLifestyleWithServiceAndSameness(Type componentType, LifestyleType lifestyle, bool overwrite,
		                                                 bool areSame)
		{
			var key = TestHandlersLifestyleWithService(componentType, lifestyle, overwrite);
			TestSameness(key, areSame);
		}

		private void TestSameness(string key, bool areSame)
		{
			var one = kernel.Resolve<IComponent>(key);
			var two = kernel.Resolve<IComponent>(key);
			if (areSame)
			{
				Assert.AreSame(one, two);
			}
			else
			{
				Assert.AreNotSame(one, two);
			}
		}

		private string TestHandlersLifestyle(Type componentType, LifestyleType lifestyle, bool overwrite)
		{
			var key = Guid.NewGuid().ToString();
			kernel.Register(Component.For(componentType).Named(key).LifeStyle.Is(lifestyle));
			var handler = kernel.GetHandler(key);
			Assert.AreEqual(lifestyle, handler.ComponentModel.LifestyleType);
			return key;
		}

		private string TestHandlersLifestyleWithService(Type componentType, LifestyleType lifestyle, bool overwrite)
		{
			var key = Guid.NewGuid().ToString();
			kernel.Register(Component.For<IComponent>().ImplementedBy(componentType).Named(key).LifeStyle.Is(lifestyle));
			var handler = kernel.GetHandler(key);
			Assert.AreEqual(lifestyle, handler.ComponentModel.LifestyleType);
			return key;
		}

		private Property ScopeRoot()
		{
			return Property.ForKey(HandlerExtensionsUtil.ResolveExtensionsKey).Eq(new IResolveExtension[] { new CustomLifestyle_Scope() });
		}

		private void OtherThread()
		{
			var handler = kernel.GetHandler("a");
			instance3 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
		}

		[Test]
		public void BadLifestyleSetProgromatically()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			                                           kernel.Register(Component.For(typeof(IComponent))
			                                                           	.ImplementedBy(typeof(TrivialComponent))
			                                                           	.Named("a")
			                                                           	.LifeStyle.Is(LifestyleType.Undefined)));
		}

		[Test]
		public void LifestyleSetProgramatically()
		{
			TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Transient, false);
			TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Singleton, false);
			TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Thread, false);
			TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Transient, false);
#if (!SILVERLIGHT)
			TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.PerWebRequest, false);
#endif

			TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Transient, false);
			TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Singleton, false);
			TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Thread, false);
			TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Transient, false);
#if (!SILVERLIGHT)
			TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.PerWebRequest, false);
#endif

			TestLifestyleAndSameness(typeof(PerThreadComponent), LifestyleType.Transient, true, false);
			TestLifestyleAndSameness(typeof(SingletonComponent), LifestyleType.Transient, true, false);
			TestLifestyleAndSameness(typeof(TransientComponent), LifestyleType.Singleton, true, true);

			TestLifestyleWithServiceAndSameness(typeof(PerThreadComponent), LifestyleType.Transient, true, false);
			TestLifestyleWithServiceAndSameness(typeof(SingletonComponent), LifestyleType.Transient, true, false);
			TestLifestyleWithServiceAndSameness(typeof(TransientComponent), LifestyleType.Singleton, true, true);
		}

		[Test]
		public void LifestyleSetThroughAttribute()
		{
			kernel.Register(Component.For(typeof(TransientComponent)).Named("a"));
			var handler = kernel.GetHandler("a");
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);

			kernel.Register(Component.For(typeof(SingletonComponent)).Named("b"));
			handler = kernel.GetHandler("b");
			Assert.AreEqual(LifestyleType.Singleton, handler.ComponentModel.LifestyleType);

			kernel.Register(Component.For(typeof(CustomComponent)).Named("c"));
			handler = kernel.GetHandler("c");
			Assert.AreEqual(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
#if (!SILVERLIGHT)
			kernel.Register(Component.For(typeof(PerWebRequestComponent)).Named("d"));
			handler = kernel.GetHandler("d");
			Assert.AreEqual(LifestyleType.PerWebRequest, handler.ComponentModel.LifestyleType);
#endif
		}

		[Test]
		public void LifestyleSetThroughExternalConfig()
		{
			IConfiguration confignode = new MutableConfiguration("component");
			confignode.Attributes.Add("lifestyle", "transient");
			kernel.ConfigurationStore.AddComponentConfiguration("a", confignode);
			kernel.Register(Component.For(typeof(TrivialComponent)).Named("a"));
			var handler = kernel.GetHandler("a");
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);

			confignode = new MutableConfiguration("component");
			confignode.Attributes.Add("lifestyle", "singleton");
			kernel.ConfigurationStore.AddComponentConfiguration("b", confignode);
			kernel.Register(Component.For(typeof(TrivialComponent)).Named("b"));
			handler = kernel.GetHandler("b");
			Assert.AreEqual(LifestyleType.Singleton, handler.ComponentModel.LifestyleType);

			confignode = new MutableConfiguration("component");
			confignode.Attributes.Add("lifestyle", "thread");
			kernel.ConfigurationStore.AddComponentConfiguration("c", confignode);
			kernel.Register(Component.For(typeof(TrivialComponent)).Named("c"));
			handler = kernel.GetHandler("c");
			Assert.AreEqual(LifestyleType.Thread, handler.ComponentModel.LifestyleType);
#if (!SILVERLIGHT)
			confignode = new MutableConfiguration("component");
			confignode.Attributes.Add("lifestyle", "perWebRequest");
			kernel.ConfigurationStore.AddComponentConfiguration("d", confignode);
			kernel.Register(Component.For(typeof(TrivialComponent)).Named("d"));
			handler = kernel.GetHandler("d");
			Assert.AreEqual(LifestyleType.PerWebRequest, handler.ComponentModel.LifestyleType);
#endif
		}

		[Test]
		public void Lifestyle_from_configuration_overwrites_attribute()
		{
			var confignode = new MutableConfiguration("component");
			confignode.Attributes.Add("lifestyle", "transient");
			kernel.ConfigurationStore.AddComponentConfiguration("a", confignode);
			kernel.Register(Component.For(typeof(SingletonComponent)).Named("a"));
			var handler = kernel.GetHandler("a");
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Lifestyle_from_fluent_registration_overwrites_attribute()
		{
			kernel.Register(Component.For<SingletonComponent>().Named("a").LifeStyle.Transient);
			var handler = kernel.GetHandler("a");
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
		}

		[Test(Description = "Prototype spike of the idea of providing scoped lifestyle - scoped per root component.")]
		public void Per_dependency_tree()
		{
			kernel.Register(
				Component.For<Root>().ExtendedProperties(ScopeRoot()),
				Component.For<Branch>(),
				Component.For<Leaf>().LifeStyle.Custom<CustomLifestyle_Scoped>()
				);
			var root = kernel.Resolve<Root>();
			Assert.AreSame(root.Leaf, root.Branch.Leaf);
		}

		[Test]
		public void TestCustom()
		{
			kernel.Register(Component.For(typeof(IComponent)).ImplementedBy(typeof(CustomComponent)).Named("a"));

			var handler = kernel.GetHandler("a");

			var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

			Assert.IsNotNull(instance1);
		}

		[Test]
		public void TestPerThread()
		{
			kernel.Register(Component.For(typeof(IComponent)).ImplementedBy(typeof(PerThreadComponent)).Named("a"));

			var handler = kernel.GetHandler("a");

			var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
			var instance2 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

			Assert.IsNotNull(instance1);
			Assert.IsNotNull(instance2);

			Assert.IsTrue(instance1.Equals(instance2));
			Assert.IsTrue(instance1.ID == instance2.ID);

			var thread = new Thread(OtherThread);
			thread.Start();
			thread.Join();

			Assert.IsNotNull(instance3);
			Assert.IsTrue(!instance1.Equals(instance3));
			Assert.IsTrue(instance1.ID != instance3.ID);
		}

		[Test]
		public void TestSingleton()
		{
			kernel.Register(Component.For(typeof(IComponent)).ImplementedBy(typeof(SingletonComponent)).Named("a"));

			var handler = kernel.GetHandler("a");

			var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
			var instance2 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

			Assert.IsNotNull(instance1);
			Assert.IsNotNull(instance2);

			Assert.IsTrue(instance1.Equals(instance2));
			Assert.IsTrue(instance1.ID == instance2.ID);
		}

		[Test]
		public void TestTransient()
		{
			kernel.Register(Component.For(typeof(IComponent)).ImplementedBy(typeof(TransientComponent)).Named("a"));

			var handler = kernel.GetHandler("a");

			var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
			var instance2 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

			Assert.IsNotNull(instance1);
			Assert.IsNotNull(instance2);

			Assert.IsTrue(!instance1.Equals(instance2));
			Assert.IsTrue(instance1.ID != instance2.ID);
		}
	}
}