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

namespace Castle.Windsor.Tests
{
	using System;

	using Castle.Core;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.XmlFiles;
	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class CircularDependencyTestCase : AbstractContainerTestCase
	{
		[Test]
		public void ShouldNotGetCircularDepencyExceptionWhenResolvingTypeOnItselfWithDifferentModels()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("IOC-51.xml")));
			Assert.IsNotNull(container.Resolve<object>("path.fileFinder"));
		}

		[Test]
		public void ShouldNotSetTheViewControllerProperty()
		{
			Container.Register(Component.For<IController>().ImplementedBy<Controller>().Named("controller"),
			                   Component.For<IView>().ImplementedBy<View>().Named("view"));
			var controller = Container.Resolve<Controller>("controller");
			Assert.IsNotNull(controller.View);
			Assert.IsNull(controller.View.Controller);
		}

		[Test]
		public void Should_not_try_to_instantiate_singletons_twice_when_circular_dependency()
		{
			SingletonComponent.CtorCallsCount = 0;
			Container.Register(Component.For<SingletonComponent>(),
			                   Component.For<SingletonDependency>());

			var component = Container.Resolve<SingletonComponent>();
			Assert.IsNotNull(component.Dependency);
			Assert.AreEqual(1, SingletonComponent.CtorCallsCount);
		}

		[Test]
		[Ignore("This is not supported. Should be?")]
		public void Should_not_try_to_instantiate_singletons_twice_when_circular_property_dependency()
		{
			SingletonPropertyComponent.CtorCallsCount = 0;
			Container.Register(Component.For<SingletonPropertyComponent>(),
			                   Component.For<SingletonPropertyDependency>());

			var component = Container.Resolve<SingletonPropertyComponent>();
			Assert.IsNotNull(component.Dependency);
			Assert.AreSame(component, component.Dependency.Component);
			Assert.AreEqual(1, SingletonPropertyComponent.CtorCallsCount);
		}

		[Test]
		public void ThrowsACircularDependencyException2()
		{
			Container.Register(Component.For<CompA>().Named("compA"),
			                   Component.For<CompB>().Named("compB"),
			                   Component.For<CompC>().Named("compC"),
			                   Component.For<CompD>().Named("compD"));

			var exception =
				Assert.Throws<CircularDependencyException>(() => Container.Resolve<CompA>("compA"));
			var expectedMessage =
				string.Format(
					"Dependency cycle has been detected when trying to resolve component 'compA'.{0}The resolution tree that resulted in the cycle is the following:{0}Component 'compA' resolved as dependency of{0}	component 'compD' resolved as dependency of{0}	component 'compC' resolved as dependency of{0}	component 'compB' resolved as dependency of{0}	component 'compA' which is the root component being resolved.{0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage, exception.Message);
		}
	}

	[Singleton]
	public class SingletonComponent
	{
		public static int CtorCallsCount;

		public SingletonComponent()
		{
			CtorCallsCount++;
		}

		public SingletonDependency Dependency { get; set; }
	}

	[Singleton]
	public class SingletonPropertyComponent
	{
		public static int CtorCallsCount;

		public SingletonPropertyComponent()
		{
			CtorCallsCount++;
		}

		public SingletonPropertyDependency Dependency { get; set; }
	}

	[Singleton]
	public class SingletonDependency
	{
		public SingletonDependency(SingletonComponent c)
		{
		}
	}

	[Singleton]
	public class SingletonPropertyDependency
	{
		public SingletonPropertyComponent Component { get; set; }
	}

	namespace IOC51
	{
		using System.Reflection;

		public interface IPathProvider
		{
			string Path { get; }
		}
		
		public class AssemblyPath : IPathProvider
		{
			public string Path
			{
				get
				{
					var uriPath = new Uri(typeof(AssemblyPath).GetTypeInfo().Assembly.CodeBase);
					return uriPath.LocalPath;
				}
			}
		}

		public class RelativeFilePath : IPathProvider
		{
			private readonly string _path;

			public RelativeFilePath(IPathProvider basePathProvider, string extensionsPath)
			{
				_path = System.IO.Path.Combine(basePathProvider.Path + "\\", extensionsPath);
			}

			public string Path
			{
				get { return _path; }
			}
		}
	}
}