// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	using System;
	using System.Threading;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.Windsor.Extensions.DependencyInjection.Interfaces.Scope;

	internal class ExtensionContainerScope : IExtensionContainerScope
	{
		public static string TransientMarker = "Transient";

		private ExtensionContainerScope()
		{
			current = new AsyncLocal<ExtensionContainerScope>();
			Parent = null;
		}

		private ExtensionContainerScope(ExtensionContainerScope parent)
		{
			current = null;
			Parent = parent;
		}

		private readonly IScopeCache scopeCache = new ScopeCache();

		public ExtensionContainerScope Root => RootInstance;
		public static ExtensionContainerScope RootInstance
		{
			get
			{
				if (rootInstance.Value == null)
				{
					return BeginRootScope();
				}
				return rootInstance.Value;
			}
			set => rootInstance.Value = value;
		}
		private static readonly AsyncLocal<ExtensionContainerScope> rootInstance = new AsyncLocal<ExtensionContainerScope>();

		public ExtensionContainerScope Parent {get; }

		private readonly AsyncLocal<ExtensionContainerScope> current;
		public ExtensionContainerScope Current
		{
			get => current.Value;
			set => current.Value = value; 
		}

		private static ExtensionContainerScope BeginRootScope()
		{
			var scope = new ExtensionContainerScope();
			RootInstance = scope;
			return scope;
		}
		
		public ExtensionContainerScope BeginScope(ExtensionContainerScope parent)
		{
			ExtensionContainerScope scope;
			if(parent == null)
			{
				scope = new ExtensionContainerScope(RootInstance);
			}
			else
			{
				scope = new ExtensionContainerScope(parent);
			}
			
			RootInstance.Current = scope;
			return scope;
		}


		public void Dispose()
		{
			if (scopeCache is IDisposable disposableCache)
			{
				disposableCache.Dispose();
			}

			if (Parent != null)
			{
				RootInstance.Current = Parent;
			}
			else if (this != RootInstance)
			{
				RootInstance.Current = RootInstance;
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

		/// <summary>
		/// Forces a specific <see name="ExtensionContainerScope" /> for 'using' block. In .NET scope is tied to an instance of <see name="System.IServiceProvider" /> not a thread or async context
		/// </summary>
		internal class ForcedScope : IDisposable
		{
			private readonly ExtensionContainerScope scope;
			private readonly ExtensionContainerScope previousScope;
			public ForcedScope(ExtensionContainerScope scope)
			{
				this.scope = scope;
				previousScope = scope.Parent;
				RootInstance.Current = scope;
			}
			public void Dispose()
			{
				RootInstance.Current = previousScope;
			}
		}
	}
}