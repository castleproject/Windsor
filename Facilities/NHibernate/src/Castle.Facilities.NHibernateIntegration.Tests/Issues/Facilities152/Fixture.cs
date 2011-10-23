using Castle.Core.Resource;
using Castle.Facilities.NHibernateIntegration.SessionStores;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NUnit.Framework;

namespace Castle.Facilities.NHibernateIntegration.Tests.Issues.Facilities152
{
    [TestFixture]
    public class Fixture
    {
        [Test]
        public void Should_Read_IsWeb_Configuration_From_Xml_Registration()
        {
            var file1 = "Castle.Facilities.NHibernateIntegration.Tests/Issues.Facilities152.facilityweb.xml";
            var file2 = "Castle.Facilities.NHibernateIntegration.Tests/Issues.Facilities152.facilitynonweb.xml";

            var containerWhenIsWebTrue = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file1)));

            var containerWhenIsWebFalse = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file2)));

            var sessionStoreWhenIsWebTrue = containerWhenIsWebTrue.Resolve<ISessionStore>();

            var sessionStoreWhenIsWebFalse = containerWhenIsWebFalse.Resolve<ISessionStore>();

            Assert.IsInstanceOf(typeof(WebSessionStore), sessionStoreWhenIsWebTrue);
            Assert.IsInstanceOf(typeof(CallContextSessionStore), sessionStoreWhenIsWebFalse);
        }
    }
}