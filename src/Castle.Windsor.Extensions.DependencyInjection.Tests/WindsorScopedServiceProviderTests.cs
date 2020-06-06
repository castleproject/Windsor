namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Specification;
    using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
    using Xunit;

    public class WindsorScopedServiceProviderTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var factory = new WindsorServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            return factory.CreateServiceProvider(container);
        }

		[Fact]
        public void RegistrationOrderIsPreservedWhenServicesAreIEnumerableResolved2()
        {
            // Arrange
            var collection = new TestServiceCollection();
            collection.AddTransient(typeof(IFakeMultipleService), typeof(FakeOneMultipleService));
            collection.AddTransient(typeof(IFakeMultipleService), typeof(FakeTwoMultipleService));

            var provider = CreateServiceProvider(collection);

            collection.Reverse();
            var providerReversed = CreateServiceProvider(collection);

            // Act
            var services = provider.GetService<IEnumerable<IFakeMultipleService>>();
            var servicesReversed = providerReversed.GetService<IEnumerable<IFakeMultipleService>>();

            // Assert
            Assert.Collection(services,
                service => Assert.IsType<FakeOneMultipleService>(service),
                service => Assert.IsType<FakeTwoMultipleService>(service));

            Assert.Collection(servicesReversed,
                service => Assert.IsType<FakeTwoMultipleService>(service),
                service => Assert.IsType<FakeOneMultipleService>(service));
        }
    }

    internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}
