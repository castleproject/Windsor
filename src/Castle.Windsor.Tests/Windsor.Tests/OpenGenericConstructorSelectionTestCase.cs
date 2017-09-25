// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Windsor.Tests
{
	using System;
	using System.Collections;

	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	// Please see DefaultDependencyResolver -> HasAnyComponentInValidState -> RebuildOpenGenericHandlersWithClosedGenericSubHandlers
	// 
	// The problem here is that DefaultGenericHandler's will always be in an HandlerState.WaitingDependency state for open generic types.
	// IOpenGeneric<> is registered in the container as an open generic handler. When resolving Control -> ctor(IOpenGeneric<ArrayList>) then the wrong
	// constructor gets chosen for the Control class.  
	// 
	// In this case the DefaultGenericHandler of type IOpenGeneric<> should create a sub DefaultGenericHandler which is bound to IOpenGeneric<ArrayList>
	// and submit whether it is resolvable or not during the DefaultDependencyResolver -> HasAnyComponentInValidState logic.

	[TestFixture]
	public class OpenGenericConstructorSelectionTestCase : AbstractContainerTestCase
	{
		[Test]
		[Bug("https://github.com/castleproject/Windsor/issues/136")]
		public void Resolves_using_most_greedy_constructor_when_having_open_generic_container_registrations_with_inferred_generic_parameters()
		{
			Container.Register(Component.For(typeof(IOpenGeneric<>)).ImplementedBy(typeof(OpenGeneric<>)));
			Container.Register(Component.For(typeof(IClosedGenericArrayList<ArrayList>)).ImplementedBy(typeof(ClosedGenericArrayList)));
			Container.Register(Component.For(typeof(Control)));
			Container.Resolve<Control>();
		}

		public class Control
		{
			// Least Greedy has no parameters, should NOT be chosen.
			public Control() { throw new Exception("The default constructor should not be chosen!"); }

			// Most greedy has parameter, should be chosen.
			public Control(IOpenGeneric<ArrayList> param) { }
		}

		public class OpenGeneric<T> : IOpenGeneric<T>
		{
			public OpenGeneric(IClosedGenericArrayList<T> param) { }
		}

		public class ClosedGenericArrayList : IClosedGenericArrayList<ArrayList> { }
		public interface IOpenGeneric<T> { }
		public interface IClosedGenericArrayList<T> { }
	}
}