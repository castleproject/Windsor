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
	public class IoC_108 : AbstractContainerTestCase
	{
		public class Service1
		{
			public void OpA()
			{
			}

			public void OpB()
			{
			}
		}

		public class Service2
		{
			public Service2(Service1 service1)
			{
				Service1 = service1;
			}

			public Service1 Service1 { get; private set; }
		}

		[Test]
		public void Should_not_fail_when_constructor_parameter_and_public_property_with_private_setter_have_same_name()
		{
			Container.Register(Component.For<Service2>(),
			                   Component.For<Service1>());

			Container.Resolve<Service2>();
		}
	}
}