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

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Specification;

	public class WindsorScopedServiceProviderTests : SkippableDependencyInjectionSpecificationTests, IDisposable
	{
		private bool _disposedValue;
		private WindsorServiceProviderFactory _factory;

		protected override IServiceProvider CreateServiceProviderImpl(IServiceCollection serviceCollection)
		{
			_factory = new WindsorServiceProviderFactory();
			var container = _factory.CreateBuilder(serviceCollection);
			return _factory.CreateServiceProvider(container);
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
