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

namespace Castle.Windsor.Tests.Handlers
{
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class EmptyConstructorTestCase : AbstractContainerTestCase
	{
		private class ExplicitRequiredDependencyDescriptor : IComponentModelDescriptor
		{
			public void BuildComponentModel(IKernel kernel, ComponentModel model)
			{
				model.Dependencies.Add(new DependencyModel("", typeof(B), false));
			}

			public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
			{
			}
		}

		private class RequirePropertyDescriptor : IComponentModelDescriptor
		{
			public void BuildComponentModel(IKernel kernel, ComponentModel model)
			{
			}

			public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
			{
				model.Requires<A>();
			}
		}

		[Test]
		public void Component_With_Explicit_Required_Dependency_Will_Be_Marked_Waiting()
		{
			Container.Register(Component.For<AProp>()
			                   	.AddDescriptor(new ExplicitRequiredDependencyDescriptor()));

			var handler = Container.Kernel.GetHandler(typeof(AProp));
			Assert.AreEqual(HandlerState.WaitingDependency, handler.CurrentState);
		}

		[Test]
		public void Component_With_Required_Properies_Will_Be_Marked_Waiting()
		{
			Container.Register(Component.For<AProp>()
			                   	.AddDescriptor(new RequirePropertyDescriptor()));

			var handler = Container.Kernel.GetHandler(typeof(AProp));
			Assert.AreEqual(HandlerState.WaitingDependency, handler.CurrentState);
		}
	}
}