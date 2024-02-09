#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using System;
using Xunit;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	public class WindsorKeyedDependencyInjectionSpecificationTests : KeyedDependencyInjectionSpecificationTests
	{
		protected override IServiceProvider CreateServiceProvider(IServiceCollection collection)
		{
			if (collection is TestServiceCollection)
			{
				var factory = new WindsorServiceProviderFactory();
				var container = factory.CreateBuilder(collection);
				return factory.CreateServiceProvider(container);
			}

            return collection.BuildServiceProvider();
		}

		//[Fact]
		//public void ResolveKeyedServiceSingletonInstanceWithAnyKey2()
		//{
		//	var serviceCollection = new ServiceCollection();
		//	serviceCollection.AddKeyedSingleton<IService, Service>(KeyedService.AnyKey);

		//	var provider = CreateServiceProvider(serviceCollection);

		//	Assert.Null(provider.GetService<IService>());

		//	var serviceKey1 = "some-key";
		//	var svc1 = provider.GetKeyedService<IService>(serviceKey1);
		//	Assert.NotNull(svc1);
		//	Assert.Equal(serviceKey1, svc1.ToString());

		//	var serviceKey2 = "some-other-key";
		//	var svc2 = provider.GetKeyedService<IService>(serviceKey2);
		//	Assert.NotNull(svc2);
		//	Assert.Equal(serviceKey2, svc2.ToString());
		//}

		internal interface IService { }

		internal class Service : IService
		{
			private readonly string _id;

			public Service() => _id = Guid.NewGuid().ToString();

			public Service([ServiceKey] string id) => _id = id;

			public override string? ToString() => _id;
		}

		internal class OtherService
		{
			public OtherService(
				[FromKeyedServices("service1")] IService service1,
				[FromKeyedServices("service2")] IService service2)
			{
				Service1 = service1;
				Service2 = service2;
			}

			public IService Service1 { get; }

			public IService Service2 { get; }
		}

		public class FakeService : IFakeEveryService, IDisposable
		{
			public PocoClass Value { get; set; }

			public bool Disposed { get; private set; }

			public void Dispose()
			{
				if (Disposed)
				{
					throw new ObjectDisposedException(nameof(FakeService));
				}

				Disposed = true;
			}
		}

		public interface IFakeEveryService :
			IFakeService,
			IFakeMultipleService,
			IFakeScopedService
		{
		}

		public interface IFakeMultipleService : IFakeService
		{
		}

		public interface IFakeScopedService : IFakeService
		{
		}

		public interface IFakeService
		{
		}

		public class PocoClass
		{
		}
	}

	//public class WindsorKeyedDependencyInjectionSpecificationExplicitContainerTests : KeyedDependencyInjectionSpecificationTests
	//{
	//	protected override IServiceProvider CreateServiceProvider(IServiceCollection collection)
	//	{
	//		var factory = new WindsorServiceProviderFactory(new WindsorContainer());
	//		var container = factory.CreateBuilder(collection);
	//		return factory.CreateServiceProvider(container);
	//	}
	//}
}

#endif
