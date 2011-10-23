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

namespace CastleTests
{
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class WindsorServiceProviderTextCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_windsor_service_provider_resolve_services()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());
			WindsorServiceProvider provider = new WindsorServiceProvider(Container);
			IEmptyService service = provider.GetService<IEmptyService>();
			Assert.IsNotNull(service);
		}

		[Test]
		public void Can_windsor_service_provider_return_null_when_service_not_found()
		{
			WindsorServiceProvider provider = new WindsorServiceProvider(Container);
			IEmptyService service = provider.GetService<IEmptyService>();
			Assert.IsNull(service);
		}
	}
}