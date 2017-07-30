#if NET45

namespace Castle.Facilities.Owin.WebHost.Lifestyles
{
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal class MsSystemWebHttpContextScopeAccessor : IScopeAccessor
	{
		private const string ScopeKey = "castle.windsor.facility.owin.scope";

		public ILifetimeScope GetScope(CreationContext context)
		{
			return RequireScope();
		}

		public static ILifetimeScope RequireScope()
		{
			return MsSystemWebHttpContextScopeWrapper.GetOrSet<ILifetimeScope>(ScopeKey, () => new MsSystemWebHttpContextLifetimeScope());
		}

		public static void ReleaseScope()
		{
			MsSystemWebHttpContextScopeWrapper.Get<ILifetimeScope>(ScopeKey).Dispose();
		}

		public void Dispose()
		{
			ReleaseScope();
		}
	}
}

#endif