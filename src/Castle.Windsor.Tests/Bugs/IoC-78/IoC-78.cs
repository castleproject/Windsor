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

namespace Castle.Windsor.Tests.Bugs.IoC_78
{
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;

	using CastleTests;

	using NUnit.Framework;

	[TestFixture]
	public class IoC78 : AbstractContainerTestCase
	{
		[Test]
		public void Will_Ignore_Components_Already_in_Dependency_Tracker_Constructor()
		{
			Container.Register(Component.For<IChain>().ImplementedBy<MyChain>().Named("chain"));
			Container.Register(Component.For<IChain>().ImplementedBy<MyChain2>().Named("chain2"));

			var resolve = Container.Resolve<IChain>("chain2");
			Assert.IsNotNull(resolve);
		}

		[Test]
		public void Will_Ignore_Components_Already_in_Dependency_Tracker_Property()
		{
			Container.Register(Component.For<IChain>().ImplementedBy<MyChain3>());

			var chain3 = (MyChain3)Container.Resolve<IChain>();
			Assert.IsNull(chain3.Chain);
		}

		[Test]
		public void Will_Not_Try_To_Resolve_Component_To_Itself()
		{
			Container.Register(Component.For<IChain>().ImplementedBy<MyChain4>());
			Assert.Throws<HandlerException>(() => Container.Resolve<IChain>());
		}
	}

	public interface IChain
	{
	}

	public class MyChain : IChain
	{
		public MyChain()
		{
		}

		public MyChain(IChain chain)
		{
		}
	}

	public class MyChain2 : IChain
	{
		public MyChain2()
		{
		}

		public MyChain2(IChain chain)
		{
		}
	}

	public class MyChain4 : IChain
	{
		public MyChain4(IChain chain)
		{
		}
	}

	public class MyChain3 : IChain
	{
		public IChain Chain { get; set; }
	}
}