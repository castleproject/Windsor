using Castle.Core;
using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CastleTests.Facilities.Startable
{
    [TestFixture]
    public class StartOnCallTestCase
    {
        [Test]
        public void startables_should_be_started_after_explicit_call_to_facility()
        {
            using(var container = new WindsorContainer())
            {
                StartableFacility facility = null;
                container.AddFacility<StartableFacility>(f => { facility = f; f.StartOnCall(); });
                container.Register(Component.For<StartableOnAction>());
                Assert.IsFalse(StartableOnAction.Called);
                facility.Start();
                Assert.IsTrue(StartableOnAction.Called);
            }
        }
    }

    public class StartableOnAction : IStartable
    {
        public static bool Called;
        
        public void Start()
        {
            Called = true;
        }

        public void Stop()
        {
            
        }
    }

}
