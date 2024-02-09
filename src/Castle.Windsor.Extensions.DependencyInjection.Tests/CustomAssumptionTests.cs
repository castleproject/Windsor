#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	public abstract class CustomAssumptionTests : IDisposable
	{
		private IServiceProvider _serviceProvider;

		[Fact]
		public void Resolve_All()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddKeyedSingleton<ITestService, TestService>("one");
			serviceCollection.AddKeyedSingleton<ITestService, AnotherTestService>("one");
			serviceCollection.AddTransient<ITestService, AnotherTestService>();
			_serviceProvider = BuildServiceProvider(serviceCollection);

			// resolve all non-keyed services
			var services = _serviceProvider.GetServices<ITestService>();
			Assert.Single(services);
			Assert.IsType<AnotherTestService>(services.First());

			// passing "null" as the key should return all non-keyed services
			var keyedServices = _serviceProvider.GetKeyedServices<ITestService>(null);
			Assert.Single(keyedServices);
			Assert.IsType<AnotherTestService>(keyedServices.First());

			// resolve all keyed services
			keyedServices = _serviceProvider.GetKeyedServices<ITestService>("one");
			Assert.Equal(2, keyedServices.Count());
			Assert.IsType<TestService>(keyedServices.First());
			Assert.IsType<AnotherTestService>(keyedServices.Last());
		}

		protected abstract IServiceCollection GetServiceCollection();

		protected abstract IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            // Dispose unmanaged resources
        }
	}

	public class RealCustomAssumptionTests : CustomAssumptionTests
	{
		protected override IServiceCollection GetServiceCollection()
		{
			return new RealTestServiceCollection();
		}

		protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
		{
			return serviceCollection.BuildServiceProvider();
		}
	}

	public class CastleWindsorCustomAssumptionTests : CustomAssumptionTests
	{
		protected override IServiceCollection GetServiceCollection()
		{
			return new TestServiceCollection();
		}

		protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
		{
			var factory = new WindsorServiceProviderFactory();
			var container = factory.CreateBuilder(serviceCollection);
			return factory.CreateServiceProvider(container);
		}
	}

	internal class TestService : ITestService
	{
	}

	internal class AnotherTestService : ITestService
	{
	}

	internal interface ITestService
	{
	}
}
#endif