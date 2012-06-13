namespace Castle.Windsor.Tests.Bugs
{
    using System;

    using Castle.DynamicProxy;
    using Castle.MicroKernel.Registration;

    using NUnit.Framework;

    [TestFixture]
    public class IoC_332
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TargetlessProxyResolveUsingFactoryMethodIsUnsupported()
        {
            var container = new WindsorContainer()
                .Register(Component.For<ServiceFactory>().ImplementedBy<ServiceFactory>(),
                          Component.For<IService>().UsingFactoryMethod(k => k.Resolve<ServiceFactory>().Create()));
            container.Resolve<IService>();
        }

        class ServiceFactory
        {
            private ProxyGenerator _proxyGenerator;

            public ServiceFactory()
            {
                _proxyGenerator = new ProxyGenerator();
            }

            public IService Create()
            {
                var service = _proxyGenerator.CreateInterfaceProxyWithoutTarget<IService>();
                return service;
            }
        }

        public interface IService { }
    }
}