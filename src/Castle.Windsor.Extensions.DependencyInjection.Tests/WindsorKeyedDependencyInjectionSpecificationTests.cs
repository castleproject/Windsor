#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using System;
using Xunit;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	public class WindsorKeyedDependencyInjectionSpecificationTests : KeyedDependencyInjectionSpecificationTests, IDisposable
	{
		private bool _disposedValue;
		private WindsorServiceProviderFactory _factory;

		protected override IServiceProvider CreateServiceProvider(IServiceCollection collection)
		{
			if (collection is TestServiceCollection)
			{
				_factory = new WindsorServiceProviderFactory();
				var container = _factory.CreateBuilder(collection);
				return _factory.CreateServiceProvider(container);
			}

            return collection.BuildServiceProvider();
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

#endif
