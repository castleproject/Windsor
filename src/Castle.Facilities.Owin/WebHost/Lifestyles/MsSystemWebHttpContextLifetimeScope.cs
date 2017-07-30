#if NET45

namespace Castle.Facilities.Owin.WebHost.Lifestyles
{
	using System;

	using Castle.Core;
	using Castle.Core.Internal;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal class MsSystemWebHttpContextLifetimeScope : ILifetimeScope
	{
		private readonly Lock @lock = Lock.Create();
		private ScopeCache cache = new ScopeCache();
		private IDisposable _innerScope;

		public MsSystemWebHttpContextLifetimeScope()
		{
			_innerScope = DependencyServiceLocator.Container.BeginScope();
		}

		public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				var burden = cache[model];
				if (burden == null)
				{
					token.Upgrade();
					burden = createInstance(delegate { });
					cache[model] = burden;
				}
				return burden;
			}
		}

		public void Dispose()
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				if (cache == null && _innerScope == null) return;
				token.Upgrade();
				cache?.Dispose();
				cache = null;
				_innerScope?.Dispose();
				_innerScope = null;
			}
		}
	}
}

#endif
