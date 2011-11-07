namespace Castle.Facilities.NHibernateIntegration.Tests.Common
{
	using NUnit.Framework;

	[NHSessionAware]
	public class BlogRepository
	{
		private readonly ISessionStore sessionStore;
		private readonly ISessionManager sessionManager;

		public BlogRepository(ISessionManager sessionManager, ISessionStore sessionStore)
		{
			this.sessionStore = sessionStore;
			this.sessionManager = sessionManager;
		}

		[NHSessionRequired]
		public virtual void FetchAll()
		{
			Assert.IsNotNull(sessionStore.FindCompatibleSession(Constants.DefaultAlias));

			sessionManager.OpenSession();
		}
	}
}
