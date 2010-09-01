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

namespace Castle.Windsor.Tests.Bugs
{
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_127
	{
		[Test]
		public void AddComponentInstanceAndChildContainers()
		{
			IWindsorContainer parent = new WindsorContainer();
			IWindsorContainer child = new WindsorContainer();
			parent.AddChildContainer(child);

			IEmptyService clock1 = new EmptyServiceA();
			IEmptyService clock2 = new EmptyServiceB();

			parent.Kernel.Register(Component.For(typeof(IEmptyService)).Instance(clock2));
			child.Kernel.Register(Component.For(typeof(IEmptyService)).Instance(clock1));

			Assert.AreSame(clock2, parent.Resolve<IEmptyService>());
			Assert.AreSame(clock1, child.Resolve<IEmptyService>());
		}
	}
}