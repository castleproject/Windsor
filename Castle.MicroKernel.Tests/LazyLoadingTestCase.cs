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

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	using NUnit.Framework;

	[TestFixture]
	public class LazyLoadingTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
			kernel.AddComponent<Loader>(typeof(ILazyComponentLoader));
		}

		[Test]
		public void Can_Lazily_resolve_component()
		{

			var service = kernel.Resolve("foo", typeof(IHasDefaultImplementation));
			Assert.IsNotNull(service);
			Assert.IsInstanceOf<Implementation>(service);
		}

		[Test]
		public void Can_lazily_resolve_dependency()
		{
			kernel.AddComponent<UsingLazyComponent>();
			var component = kernel.Resolve<UsingLazyComponent>();
			Assert.IsNotNull(component.Dependency);
		}

		[Test]
		[Ignore("Not sure how to do this in an elegant way... plus I don't think it's the right place for this anyway.")]
		public void Can_lazily_resolve_parameters()
		{
			kernel.AddComponent<UsingString>();
			var component = kernel.Resolve<UsingString>();
			Assert.AreEqual("Foo", component.Parameter);
		}
	}

	public class Loader : ILazyComponentLoader
	{

		public IRegistration Load(string key, Type service)
		{
			if (!Attribute.IsDefined(service, typeof(DefaultImplementationAttribute)))
			{
				return null;
			}

			var attributes = service.GetCustomAttributes(typeof(DefaultImplementationAttribute), false);
			var attribute = attributes[0] as DefaultImplementationAttribute;
			return Component.For(service).ImplementedBy(attribute.Implementation).Named(key);
		}
	}

	public class UsingString
	{
		private readonly string parameter;

		public UsingString(string parameter)
		{
			this.parameter = parameter;
		}

		public string Parameter
		{
			get { return parameter; }
		}
	}


	public class UsingLazyComponent
	{
		private IHasDefaultImplementation dependency;

		public UsingLazyComponent(IHasDefaultImplementation dependency)
		{
			this.dependency = dependency;
		}

		public IHasDefaultImplementation Dependency
		{
			get { return dependency; }
		}
	}

	[DefaultImplementation(typeof(Implementation))]
	public interface IHasDefaultImplementation
	{
		void Foo();
	}

	public class Implementation : IHasDefaultImplementation
	{
		public void Foo()
		{
			
		}
	}

	[AttributeUsage(AttributeTargets.Interface,AllowMultiple = false)]
	public class DefaultImplementationAttribute:Attribute
	{
		private readonly Type implementation;

		public DefaultImplementationAttribute(Type implementation)
		{
			this.implementation = implementation;
		}

		public Type Implementation
		{
			get { return implementation; }
		}
	}
}