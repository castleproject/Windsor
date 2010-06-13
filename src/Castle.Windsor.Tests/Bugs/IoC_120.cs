using System;
using Castle.MicroKernel.ComponentActivator;
using NUnit.Framework;

namespace Castle.Windsor.Tests.Bugs
{
    using Castle.MicroKernel.Registration;

    [TestFixture]
    public class IoC_120
    {
        [Test]
        public void Can_resolve_component_with_internal_ctor()
        {
            var container = new WindsorContainer();
            ((IWindsorContainer)container).Register(Component.For<Foo>());
            ((IWindsorContainer)container).Register(Component.For<Bar>());

            try
            {
                container.Resolve<Bar>();
                Assert.Fail();
            }
            catch (ComponentActivatorException e)
            {
            	var expected = 
#if (!SILVERLIGHT)
				"Could not find a public constructor for the type Castle.Windsor.Tests.Bugs.Bar";
#else
				"Could not find a constructor for the type Castle.Windsor.Tests.Bugs.Bar. Make sure that it is there and that it is public.";
#endif

				Assert.AreEqual(expected,
            	                e.InnerException.Message);
            }
        }
    }

    public class Foo
    {
        
    }

    public class Bar
    {
        internal Bar(Foo f)
        {
            
        }
    }
}