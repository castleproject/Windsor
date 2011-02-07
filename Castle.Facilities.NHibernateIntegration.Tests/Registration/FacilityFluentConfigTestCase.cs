using Castle.Facilities.NHibernateIntegration.SessionStores;
using Castle.Core.Resource;
using Castle.MicroKernel.Facilities;
using NUnit.Framework;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace Castle.Facilities.NHibernateIntegration.Tests.Registration
{
    [TestFixture]
    public class FacilityFluentConfigTestCase
    {
        [Test]
        public void Should_override_DefaultConfigurationBuilder()
        {
            var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

            var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.ConfigurationBuilder<TestConfigurationBuilder>());

            Assert.AreEqual(typeof(TestConfigurationBuilder), container.Resolve<IConfigurationBuilder>().GetType());
        }

        [Test]
        public void Should_override_IsWeb()
        {
            var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

            var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.IsWeb());

            var sessionStore = container.Resolve<ISessionStore>();

            Assert.IsInstanceOf(typeof(WebSessionStore), sessionStore);
        }

        [Test, ExpectedException(typeof(FacilityException))]
        public void Should_not_accept_non_implementors_of_IConfigurationBuilder_for_override()
        {
            var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

            var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.ConfigurationBuilder(GetType()));
        }
    }
}
