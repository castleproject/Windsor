using NUnit.Framework;

namespace Castle.MicroKernel.Tests.Bugs
{
    using System;

    using Castle.MicroKernel.Registration;

    [TestFixture]
    public class IoC_114
    {

        public interface IService1
        {
        }

        public class Service1 : IService1
        {
        }

        public interface IService2
        {
        }

        public class Service2 : IService2
        {
            private IService1 _s;

            public IService1 S
            {
                get { return _s; }
                private set { _s = value; }
            }
        }

        [Test]
        public void UsingPropertyWithPrivateSetter()
        {
            IKernel container = new DefaultKernel();

            container.Register(Component.For(typeof(IService1)).ImplementedBy(typeof(Service1)).Named("service1"));
            container.Register(Component.For(typeof(IService2)).ImplementedBy(typeof(Service2)).Named("service2"));

            Service2 service2 = (Service2)container.Resolve<IService2>();

            Assert.IsNull(service2.S,"Kernel should ignore private setter properties");
        }
    }
}