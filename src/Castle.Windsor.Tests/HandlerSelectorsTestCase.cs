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
namespace Castle.Windsor.Tests
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class HandlerSelectorsTestCase
	{
		[Test]
		public void SelectUsingBusinessLogic_DirectSelection()
		{
			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher")).Register(
				Component.For<IWatcher>().ImplementedBy<SatiWatcher>().Named("astronomy.watcher"));
			var selector = new WatcherSelector();
			container.Kernel.AddHandlerSelector(selector);

			Assert.IsInstanceOf(typeof(BirdWatcher), container.Resolve<IWatcher>(), "default");
			selector.Interest = Interest.Astronomy;
			Assert.IsInstanceOf(typeof(SatiWatcher), container.Resolve<IWatcher>(), "change-by-context");
			selector.Interest = Interest.Biology;
			Assert.IsInstanceOf(typeof(BirdWatcher), container.Resolve<IWatcher>(), "explicit");
		}

		[Test]
		public void SelectUsingBusinessLogic_SubDependency()
		{
			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For(typeof(Person)).LifeStyle.Is(LifestyleType.Transient)).Register(
				Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher")).Register(
					Component.For<IWatcher>().ImplementedBy<SatiWatcher>().Named("astronomy.watcher"));
			var selector = new WatcherSelector();
			container.Kernel.AddHandlerSelector(selector);

			Assert.IsInstanceOf(typeof(BirdWatcher), container.Resolve<Person>().Watcher, "default");
			selector.Interest = Interest.Astronomy;
			Assert.IsInstanceOf(typeof(SatiWatcher), container.Resolve<Person>().Watcher, "change-by-context");
			selector.Interest = Interest.Biology;
			Assert.IsInstanceOf(typeof(BirdWatcher), container.Resolve<Person>().Watcher, "explicit");
		}

		[Test]
		public void SubDependencyResolverHasHigherPriorityThanHandlerSelector()
		{
			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For(typeof(Person)).LifeStyle.Is(LifestyleType.Transient)).Register(
				Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher")).Register(
					Component.For<IWatcher>().ImplementedBy<SatiWatcher>().Named("astronomy.watcher"));
			var selector = new WatcherSelector();
			container.Kernel.AddHandlerSelector(selector);
			container.Kernel.Resolver.AddSubResolver(new WatchSubDependencySelector());

			selector.Interest = Interest.Biology;
			Assert.IsInstanceOf(typeof(SatiWatcher), container.Resolve<Person>().Watcher,
			                    "sub dependency should resolve sati");
			Assert.IsInstanceOf(typeof(BirdWatcher), container.Resolve<IWatcher>(), "root dependency should resolve bird");
		}

		public enum Interest
		{
			None,
			Biology,
			Astronomy
		}

		public class BirdWatcher : IWatcher
		{
			public event Action<string> OnSomethingInterestingToWatch = delegate { };
		}

		public interface IWatcher
		{
			event Action<string> OnSomethingInterestingToWatch;
		}

		public class PeopleWatcher
		{
			private Person p;

			public PeopleWatcher(Person p)
			{
				this.p = p;
			}
		}

		public class Person
		{
			public IWatcher Watcher;

			public Person(IWatcher watcher)
			{
				Watcher = watcher;
			}
		}

		public class SatiWatcher : IWatcher
		{
			public event Action<string> OnSomethingInterestingToWatch = delegate { };
		}

		public class WatchSubDependencySelector : ISubDependencyResolver
		{
			public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
			                       DependencyModel dependency)
			{
				return dependency.TargetType == typeof(IWatcher);
			}

			public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
			                      DependencyModel dependency)
			{
				return new SatiWatcher();
			}
		}

		public class WatcherSelector : IHandlerSelector
		{
			public Interest Interest = Interest.None;

			public bool HasOpinionAbout(string key, Type service)
			{
				return Interest != Interest.None && service == typeof(IWatcher);
			}

			public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
			{
				foreach (var handler in handlers)
				{
					if (handler.ComponentModel.Name.ToUpper().Contains(Interest.ToString().ToUpper()))
					{
						return handler;
					}
				}
				return null;
			}
		}
	}
}