using Castle.Core.Configuration;
using Castle.Facilities.NHibernateIntegration.SessionStores;
using Castle.Core.Resource;
using Castle.MicroKernel.Facilities;
using NHibernate.Cfg;
using NUnit.Framework;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace Castle.Facilities.NHibernateIntegration.Tests.Registration
{
    [TestFixture]
    public class FacilityFluentConfigTestCase
    {
        [Test]
        public void Should_be_able_to_revolve_ISessionManager_when_fluently_configured()
        {
            var container = new WindsorContainer();

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.ConfigurationBuilder<TestConfigurationBuilder>());

            var sessionManager = container.Resolve<ISessionManager>();
            sessionManager.OpenSession();
            Assert.AreEqual(typeof(TestConfigurationBuilder), container.Resolve<IConfigurationBuilder>().GetType());
        }

        [Test]
        public void Should_override_DefaultConfigurationBuilder()
        {
            var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

            var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.ConfigurationBuilder<DummyConfigurationBuilder>());

            Assert.AreEqual(typeof(DummyConfigurationBuilder), container.Resolve<IConfigurationBuilder>().GetType());
        }

        [Test]
        public void Should_override_IsWeb()
        {
            var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

            var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.IsWeb().ConfigurationBuilder<DummyConfigurationBuilder>());

            var sessionStore = container.Resolve<ISessionStore>();

            Assert.IsInstanceOf(typeof(CallContextSessionStore), sessionStore);
        }

        [Test, ExpectedException(typeof(FacilityException))]
        public void Should_not_accept_non_implementors_of_IConfigurationBuilder_for_override()
        {
            var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

            var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

            container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.ConfigurationBuilder(GetType()));
        }
    }


    class DummyConfigurationBuilder : IConfigurationBuilder
    {
        public Configuration GetConfiguration(IConfiguration config)
        {
            return new Configuration();
        }
    }
}
