namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	using System;
	
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal  abstract class ExtensionContainerScopeBase : ILifetimeScope, IDisposable
	{
		public static string TransientMarker = "Transient";
		private readonly IScopeCache scopeCache;

		protected ExtensionContainerScopeBase()
		{
			scopeCache = new ScopeCache();
		}

		internal virtual ExtensionContainerScopeBase RootScope { get; set; }

		public virtual void Dispose()
		{
			if (scopeCache is IDisposable disposableCache)
			{
				disposableCache.Dispose();
			}
		}

		public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
		{
			lock (scopeCache)
			{
				// Add transient's burden to scope so it gets released
				if (model.Configuration.Attributes.Get(TransientMarker) == bool.TrueString)
				{
					var transientBurden = createInstance((_) => {});
					scopeCache[transientBurden] = transientBurden;
					return transientBurden;
				}

				var scopedBurden = scopeCache[model];
				if (scopedBurden != null)
				{
					return scopedBurden;
				}
				scopedBurden = createInstance((_) => {});
				scopeCache[model] = scopedBurden;
				return scopedBurden;
			}
		}
	}
}