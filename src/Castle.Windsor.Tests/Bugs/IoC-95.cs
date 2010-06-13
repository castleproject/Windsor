using System;
using Castle.Facilities.Startable;
using NUnit.Framework;

namespace Castle.MicroKernel.Tests.Bugs
{
    using Castle.MicroKernel.Registration;

    [TestFixture]
    public class IoC_95
    {
        [Test]
        public void AddingComponentToRootKernelWhenChildKernelHasStartableFacility()
        {
            IKernel kernel = new DefaultKernel();
            IKernel childKernel = new DefaultKernel();
            kernel.AddChildKernel(childKernel);
            childKernel.AddFacility("StartableFacility", new StartableFacility());
            kernel.Register(Component.For(typeof(String)).Named("string")); // exception here
        }
    }
}