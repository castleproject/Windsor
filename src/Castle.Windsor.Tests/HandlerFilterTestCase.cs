// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests
{
	using System;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class HandlerFilterTestCase : AbstractContainerTestCase
	{
		private class FailIfCalled : IHandlersFilter
		{
			public bool HasOpinionAbout(Type service)
			{
				return false;
			}

			public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
			{
				Assert.Fail("SelectHandlers was called with {0}", service);
				return null; //< could not compile without returning anything
			}
		}

		private class ReturnAllHandlersFilter : IHandlersFilter
		{
			public bool HasOpinionAbout(Type service)
			{
				return true;
			}

			public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
			{
				return handlers;
			}
		}

		private class DelegatingFilter : IHandlersFilter
		{
			private readonly Func<IHandler, bool> filter;
			private readonly Type typeToFilter;
			private IHandler[] handlersAsked;

			public DelegatingFilter(Type typeToFilter, Func<IHandler, bool> filter = null)
			{
				this.typeToFilter = typeToFilter;
				this.filter = filter ?? (t => true);
			}

			public IHandler[] HandlersAsked
			{
				get { return handlersAsked; }
			}

			public bool HasOpinionAbout(Type service)
			{
				return service == typeToFilter;
			}

			public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
			{
				handlersAsked = handlers;
				return handlers.Where(filter).ToArray();
			}
		}

		private class FilterThatRemovedFourthTaskAndOrdersTheRest : IHandlersFilter
		{
			public bool HasOpinionAbout(Type service)
			{
				return service == typeof(ISomeTask);
			}

			public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
			{
				return handlers
					.Where(h => h.ComponentModel.Implementation != typeof(Task4))
					.OrderBy(h => h.ComponentModel.Implementation.Name)
					.ToArray();
			}
		}

		private class FirstImplementation : ISomeService
		{
		}

		private interface ISomeService
		{
		}

		private interface ISomeTask
		{
		}

		private interface IUnimportantService
		{
		}

		private class SecondImplementation : ISomeService
		{
		}

		private class Task1 : ISomeTask
		{
		}

		private class Task2 : ISomeTask
		{
		}

		private class Task3 : ISomeTask
		{
		}

		private class Task4 : ISomeTask
		{
		}

		private class Task5 : ISomeTask
		{
		}

		private class TestHandlersFilter : IHandlersFilter
		{
			public bool OpinionWasChecked { get; set; }

			public bool HasOpinionAbout(Type service)
			{
				Assert.That(OpinionWasChecked, Is.False, "Opinion should not be checked more than once");

				var wasExpectedService = service == typeof(ISomeService);
				Assert.That(wasExpectedService, Is.True, "Did not expect {0} to be checked with this handler filter");

				OpinionWasChecked = true;

				return wasExpectedService;
			}

			public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
			{
				return handlers;
			}
		}

		private class ThirdImplementation : ISomeService
		{
		}

		private class UnimportantImpl : IUnimportantService
		{
		}

		[Test]
		public void Filter_gets_all_assignable_handlers_not_exiplicitly_registered_for_given_service()
		{
			Container.Register(Component.For<Task5>(),
			                   Component.For<Task3>(),
			                   Component.For<Task2>(),
			                   Component.For<Task4>(),
			                   Component.For<Task1>());

			Container.Kernel.AddHandlersFilter(new ReturnAllHandlersFilter());

			var instances = Container.ResolveAll<ISomeTask>();

			Assert.AreEqual(5, instances.Length);
		}

		[Test]
		public void Filter_gets_open_generic_handlers_when_generic_service_requested()
		{
			Container.Register(Component.For<IGeneric<A>>().ImplementedBy<GenericImpl1<A>>(),
			                   Component.For(typeof(GenericImpl2<>)));
			var filter = new DelegatingFilter(typeof(IGeneric<A>));
			Kernel.AddHandlersFilter(filter);

			Container.ResolveAll<IGeneric<A>>();

			Assert.AreEqual(2, filter.HandlersAsked.Length);
		}

		[Test]
		public void Filter_returning_empty_collection_respected()
		{
			Container.Register(Component.For<ISomeTask>().ImplementedBy<Task5>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task4>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task3>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task2>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task1>());

			Container.Kernel.AddHandlersFilter(new DelegatingFilter(typeof(ISomeTask), h => false));

			var instances = Container.ResolveAll(typeof(ISomeTask));

			Assert.IsEmpty(instances);
		}

		[Test]
		public void HandlerFilterGetsCalledLikeExpected()
		{
			Container.Register(Component.For<ISomeService>().ImplementedBy<FirstImplementation>(),
			                   Component.For<ISomeService>().ImplementedBy<SecondImplementation>(),
			                   Component.For<ISomeService>().ImplementedBy<ThirdImplementation>());

			var filter = new TestHandlersFilter();
			Container.Kernel.AddHandlersFilter(filter);

			Container.ResolveAll<ISomeService>();

			Assert.IsTrue(filter.OpinionWasChecked, "Filter's opinion should have been checked once for each handler");
		}

		[Test]
		public void HandlerFiltersPrioritizationAndOrderingIsRespected()
		{
			Container.Register(Component.For<ISomeTask>().ImplementedBy<Task5>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task3>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task2>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task4>(),
			                   Component.For<ISomeTask>().ImplementedBy<Task1>());

			Container.Kernel.AddHandlersFilter(new FilterThatRemovedFourthTaskAndOrdersTheRest());

			var instances = Container.ResolveAll(typeof(ISomeTask));

			Assert.That(instances, Has.Length.EqualTo(4));
		}

		[Test]
		public void SelectionMethodIsNeverCalledOnFilterWhenItDoesNotHaveAnOpinionForThatService()
		{
			Container.Register(Component.For<IUnimportantService>().ImplementedBy<UnimportantImpl>());

			Container.Kernel.AddHandlersFilter(new FailIfCalled());

			Container.ResolveAll(typeof(IUnimportantService));
		}
	}
}