namespace Castle.Windsor.Tests.Bugs
{
    using System.Collections;

    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Handlers;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Conversion;

    using NUnit.Framework;

    [TestFixture]
    public class IoC_325
    {
        [Test]
        public void TryResolvingViaChildKernelShouldNotThrowException()
        {
            using (var parentKernel = new DefaultKernel())
            using (var childKernel = new DefaultKernel())
            {
                parentKernel.Register(Component.For<Service>());
                parentKernel.AddChildKernel(childKernel);
                var handler = childKernel.GetHandler(typeof(Service));

                // Assert setup invariant
                Assert.IsInstanceOf<ParentHandlerWithChildResolver>(handler);
                
                var converter = parentKernel.GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager;
                var context = new CreationContext(handler, parentKernel.ReleasePolicy, typeof(Service), new Hashtable(), converter, null);
                
                Assert.DoesNotThrow(() => handler.TryResolve(context));
            }
        }

        public class Dependency
        {
        }

        public class Service
        {
            private Dependency _dependency;

            public Service(Dependency dependency)
            {
                _dependency = dependency;
            }

            public Dependency Dependency
            {
                get
                {
                    return _dependency;
                }
                set
                {
                    _dependency = value;
                }
            }
        }
    }
}