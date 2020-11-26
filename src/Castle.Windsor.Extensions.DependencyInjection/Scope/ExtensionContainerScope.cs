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
	
	internal class ExtensionContainerScope : ILifetimeScope, IDisposable
	{
		public ExtensionContainerScope Current
		{
			get => current.Value;
			set => current.Value = value; 
		}
		private AsyncLocal<ExtensionContainerScope> current  {get; set;}
		

		public static string TransientMarker = "Transient";
		
		private readonly IScopeCache scopeCache = new ScopeCache();

		internal ExtensionContainerScope RootScope {get; private set;}
		internal ExtensionContainerScope Parent {get; private set;}


		public static ExtensionContainerScope Instance
		{
			get => instance.Value;
			set => instance.Value = value;
		}

		private static readonly AsyncLocal<ExtensionContainerScope> instance = new AsyncLocal<ExtensionContainerScope>();

		public static ExtensionContainerScope BeginRootScope()
		{
			var scope = new ExtensionContainerScope
			{
				current = new AsyncLocal<ExtensionContainerScope>()
			};
			scope.RootScope = scope;
			Instance = scope;
			return scope;
		}
		
		public ExtensionContainerScope BeginScope(ExtensionContainerScope parent)
		{
			var scope = new ExtensionContainerScope();
			if(parent == null)
			{
				scope.Parent = RootScope;
			}
			else
			{
				scope.Parent = parent;
			}

			if (Instance == null)
			{
				Instance = RootScope;
			}
			scope.RootScope = RootScope;
			scope.RootScope.Current = scope;
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
				RootScope.Current = Parent;
			}
			else if (this != RootScope)
			{
				RootScope.Current = RootScope;
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
				scope.RootScope.Current = scope;
			}
			public void Dispose()
			{
				scope.RootScope.Current = previousScope;
			}
		}
	}
}