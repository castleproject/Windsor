using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Castle.Bugs
{
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;

    using NUnit.Framework;

    [TestFixture]
    public class IoC_267
    {
        [Test]
        public void When_attemting_to_resolve_component_with_nonpublic_ctor_should_throw_meaningfull_exception()
        {
            var kernel = new DefaultKernel();

            
            kernel.Register(Component.For<Int32>());
        }
    }
}
