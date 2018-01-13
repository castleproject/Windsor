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
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDisposableTestCase : AbstractContainerTestCase
	{
		private ServiceInstaller serviceInstaller;

		protected override void AfterContainerCreated()
		{
			serviceInstaller = new ServiceInstaller();
			Container.Install(serviceInstaller);
		}

		[Test]
		public void Can_resolve_service_using_factory_that_implements_disposable()
		{
			var a = Container.Resolve<IDatabaseConnectionExecutor>();
			Assert.That(a, Is.Not.Null);
		}

		public class ServiceInstaller : IWindsorInstaller
		{
			public void Install(IWindsorContainer container, IConfigurationStore store)
			{
				container.Register(
					Component.For<ISomeConnectionService>()
						.ImplementedBy<SomeConnectionService>()
						.LifestyleTransient());

				container.Register(
					Component.For<FailoverDatabaseConnectionExecutor>()
						.LifestyleTransient());

				container.Register(Component.For<IDatabaseConnectionExecutor>()
					.UsingFactoryMethod<IDatabaseConnectionExecutor>((k) => k.Resolve<FailoverDatabaseConnectionExecutor>())
					.LifestyleTransient()
					.IsDefault());
			}

		}

		public interface ISomeConnectionService
		{
		}

		public class SomeConnectionService : ISomeConnectionService, IDisposable
		{
			public void Dispose()
			{
			}
		}

		public interface IDatabaseConnectionExecutor
		{
		}

		public class FailoverDatabaseConnectionExecutor : IDatabaseConnectionExecutor
		{
			private readonly ISomeConnectionService _someConnectionService;

			public FailoverDatabaseConnectionExecutor(ISomeConnectionService someConnectionService)
			{
				this.InstanceId = Guid.NewGuid();
				_someConnectionService = someConnectionService;
			}

			public Guid InstanceId { get; set; }
		}
	}
}