// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	using System;
	using System.Collections.Generic;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Specification;
	using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
	using Xunit;

	/// <summary>
	/// This test inherits from a test class of the framework that contains a set of base tests to verify various assumption
	/// that must be satisfied by DependencyInjection.
	/// To debug a single test, open the corresponding test in dotnet/runtime repository,
	/// then copy the test here, change name and execute with debugging etc etc.
	/// This helps because source link support seems to be not to easy to use from the test runner
	/// and this tricks makes everything really simpler.
	/// </summary>
	public class WindsorScopedServiceProviderCustomWindsorContainerTests : SkippableDependencyInjectionSpecificationTests, IDisposable
	{
		private bool _disposedValue;
		private WindsorServiceProviderFactory _factory;

		protected override IServiceProvider CreateServiceProviderImpl(IServiceCollection serviceCollection)
		{
			_factory = new WindsorServiceProviderFactory(new WindsorContainer());
			var container = _factory.CreateBuilder(serviceCollection);
			return _factory.CreateServiceProvider(container);
		}

		[Fact]
		public void CreateServiceProvider_registers_provider_infrastructure_when_builder_was_not_called()
		{
			using (var container = new WindsorContainer())
			using (var factory = new WindsorServiceProviderFactory(container))
			{
				var provider = factory.CreateServiceProvider(container);
				try
				{
					Assert.NotNull(provider);
					Assert.Same(container, provider.GetRequiredService<IWindsorContainer>());
					Assert.NotNull(provider.GetRequiredService<IServiceScopeFactory>());
				}
				finally
				{
					(provider as IDisposable)?.Dispose();
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_factory?.Dispose();
				}

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
