// Copyright 2004-2024 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class PropertyCycleTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Singleton_property_cycle_should_not_return_wrong_component_from_instance_stash()
		{
			// This test reproduces a bug where TryGetHandlerFromKernel returns a cycling handler
			// when no non-cycling alternative exists, instead of setting handler = null.
			// When the property is required (non-optional), CanResolve is not called, so the
			// code path goes directly through TryGetHandlerFromKernel → ResolveCore →
			// GetContextInstance, which reads the wrong instance from the InstanceStash.
			//
			// The chain is:
			//   Resolve IState<string> (StateComponent<string>)
			//     → StateComponent ctor needs IHasState<string> (PublisherWithState<string>)
			//       → PublisherWithState is created and stashed in the CreationContext
			//       → PublisherWithState.State property (IState<string>) is resolved
			//         → Handler for IState<string> is StateComponent, which IS cycling
			//         → TryGetHandlerFromKernel finds no non-cycling alternative
			//         → BUG: returns true with the cycling handler
			//         → ResolveCore reads InstanceStash → gets PublisherWithState (wrong type!)
			//         → SetValue fails with type mismatch → ComponentActivatorException
			Container.Register(
				Component.For<IHasState<string>>().ImplementedBy<PublisherWithState<string>>()
					.LifestyleSingleton()
					.PropertiesRequire(p => p.Name == "State"),
				Component.For<IState<string>>().ImplementedBy<StateComponent<string>>().LifestyleSingleton());

			// Before the fix: ComponentActivatorException wrapping InvalidCastException
			// ("Object of type 'PublisherWithState`1[String]' cannot be converted to type 'IState`1[String]'")
			// After the fix: CircularDependencyException with a proper cycle message
			Assert.Throws<CircularDependencyException>(() => Container.Resolve<IState<string>>());
		}

		[Test]
		public void Singleton_optional_property_cycle_should_leave_property_null()
		{
			// When properties are optional (the default), CanResolve detects the cycle
			// and skips the property (left null).
			Container.Register(
				Component.For<IHasState<string>>().ImplementedBy<PublisherWithState<string>>().LifestyleSingleton(),
				Component.For<IState<string>>().ImplementedBy<StateComponent<string>>().LifestyleSingleton());

			var state = (StateComponent<string>)Container.Resolve<IState<string>>();

			Assert.IsNotNull(state);
			Assert.IsNotNull(state.Publisher);
			Assert.IsNull(((PublisherWithState<string>)state.Publisher).State,
				"Optional cycling property should be left null.");
		}

		[Test]
		public void Singleton_property_cycle_with_concrete_types_should_leave_cycling_property_null()
		{
			// ACycleProp.Prop → BCycleProp, BCycleProp.Prop → ACycleProp (property-only cycle).
			// Both properties are optional, so the cycling property should be left null.
			Container.Register(
				Component.For<ACycleProp>().LifestyleSingleton(),
				Component.For<BCycleProp>().LifestyleSingleton());

			var a = Container.Resolve<ACycleProp>();

			Assert.IsNotNull(a);
			Assert.IsNotNull(a.Prop, "Non-cycling direction should resolve normally.");
			Assert.IsNull(a.Prop.Prop,
				"Cycling property should be left null, not set to the wrong component instance.");
		}
	}
}
