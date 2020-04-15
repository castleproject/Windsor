namespace Castle.Windsor.Extensions.MsDependencyInjection.Tests
{
    using System;
    
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Specification;
    
    public class WindsorScopedServiceProviderTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var factory = new WindsorServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            return factory.CreateServiceProvider(container);
        }
    }
}
