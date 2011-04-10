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

namespace CastleTests.Bugs
{
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_114 : AbstractContainerTestCase
	{
		public interface IService1
		{
		}

		public class Service1 : IService1
		{
		}

		public interface IService2
		{
		}

		public class Service2 : IService2
		{
			public IService1 S { get; private set; }
		}

		[Test]
		public void UsingPropertyWithPrivateSetter()
		{
			Container.Register(Component.For<IService1>().ImplementedBy<Service1>(),
			                   Component.For<IService2>().ImplementedBy<Service2>());

			var service2 = (Service2)Container.Resolve<IService2>();

			Assert.IsNull(service2.S, "Kernel should ignore private setter properties");
		}
	}
}