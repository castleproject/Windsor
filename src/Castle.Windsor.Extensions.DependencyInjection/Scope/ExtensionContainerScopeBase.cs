namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	using System;
	using System.Threading;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal  abstract class ExtensionContainerScopeBase : ILifetimeScope, IDisposable
	{
		protected static readonly AsyncLocal<ExtensionContainerScopeBase> current = new AsyncLocal<ExtensionContainerScopeBase>();
		public static string TransientMarker = "Transient";
		private readonly IScopeCache scopeCache;

		protected ExtensionContainerScopeBase()
		{
			scopeCache = new ScopeCache();
		}

		/// <summary>Current scope for the thread. Initial scope will be set when calling BeginRootScope from a ExtensionContainerRootScope instance.</summary>
		/// <exception cref="InvalidOperationException">Thrown when there is no scope available.</exception>
		internal static ExtensionContainerScopeBase Current
		{
			get => current.Value ?? throw new InvalidOperationException("No scope available");
			set => current.Value = value;
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