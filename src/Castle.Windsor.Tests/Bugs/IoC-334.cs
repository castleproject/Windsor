#pragma warning disable 618
using System;

namespace CastleTests.Bugs 
{
    using Castle.Core.Configuration;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Tests.ClassComponents;

    using NUnit.Framework;

    [TestFixture]
    public class IoC_334 
    {
        [Test]
        public void FacilityConfig_is_not_null()
        {
            using (var c = new DefaultKernel())
            {
                const string facilityKey = "hiper";
                var config = new MutableConfiguration("facility");
                c.ConfigurationStore.AddFacilityConfiguration(facilityKey, config);
                var facility = new HiperFacility();
                c.AddFacility(facilityKey, facility);
                Assert.IsTrue(facility.Initialized);
            }
        }
    }
}
