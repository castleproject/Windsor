namespace Castle.Windsor.Tests.Bugs
{
    using System;
    using System.Linq;

    using Castle.MicroKernel;
    using Castle.MicroKernel.ModelBuilder.Inspectors;
    using Castle.MicroKernel.Registration;

    using NUnit.Framework;

    [TestFixture]
    public class IoC_324
    {
        [Test]
        public void ParentKernelShouldNotThrowWhenResolvingViaChildKernelOwningDependency()
        {
            using (var parentKernel = new DefaultKernel())
            using (var childKernel = new DefaultKernel())
            {
                removePropertyModelInspector(parentKernel);
                parentKernel.Register(Component.For<Service>());

                removePropertyModelInspector(childKernel);
                childKernel.Register(Component.For<Dependency>());

                parentKernel.AddChildKernel(childKernel);

                Assert.DoesNotThrow(() => childKernel.Resolve<Service>());
            }
        }

        private static void removePropertyModelInspector(IKernel kernel)
        {
            var propertyModelInspector =
                kernel.ComponentModelBuilder.Contributors.OfType<PropertiesDependenciesModelInspector>().FirstOrDefault();
            kernel.ComponentModelBuilder.RemoveContributor(propertyModelInspector);
        }

        public class Dependency
        {
        }

        public class Service
        {
            private Dependency _dependency;

            //public Service()
            //{
            //}

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