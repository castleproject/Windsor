namespace Castle.Facilities.NHibernateIntegration.Tests
{
	using Common;
	using MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class SessionManagementInterceptorsTestCase : AbstractNHibernateTestCase
	{
		[Test]
		public void SessionRequiredAttr_should_automatically_open_a_Session_under_the_hood()
		{
			container.Register(Component.For<BlogRepository>());

			container.Resolve<BlogRepository>().FetchAll();

			Assert.IsNull(container.Resolve<ISessionStore>().FindCompatibleSession(Constants.DefaultAlias));
		}
	}
}
