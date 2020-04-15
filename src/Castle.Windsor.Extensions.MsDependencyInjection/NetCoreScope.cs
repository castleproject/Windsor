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

namespace Castle.Windsor.Extensions.MsDependencyInjection
{
    using System;
    using System.Threading;

    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Lifestyle.Scoped;
    
    internal class NetCoreScope : ILifetimeScope, IDisposable
    {
        public static NetCoreScope Current => current.Value;
        public static string NetCoreTransientMarker = "NetCoreTransient";
        protected static readonly AsyncLocal<NetCoreScope> current = new AsyncLocal<NetCoreScope>();
        private readonly NetCoreRootScope rootScope;
        private readonly NetCoreScope parent;
		private readonly IScopeCache scopeCache;

       
        public virtual NetCoreRootScope RootScope => rootScope;
        public virtual int Nesting {get; private set;}

        protected NetCoreScope(NetCoreScope parent)
        {
            parent = parent;
            scopeCache = new ScopeCache();
            Nesting = (parent?.Nesting ?? 0) + 1;
            rootScope = parent?.RootScope;
        }

        public static NetCoreScope BeginScope(NetCoreScope parent)
        {
            var scope = new NetCoreScope(parent);
            current.Value = scope;
            return scope;
        }


        public void Dispose()
		{
			var disposableCache = scopeCache as IDisposable;
			if (disposableCache != null)
			{
				disposableCache.Dispose();
			}

            current.Value = parent;
		}

        public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
        {
            if(model.Configuration.Attributes.Get(NetCoreTransientMarker) == Boolean.TrueString ){
                var burder = createInstance((_) => {});
                scopeCache[burder] = burder;
                return burder;
            }
            else
            {
                var burden = scopeCache[model];
                if (burden == null)
                {
                    scopeCache[model] = burden = createInstance((_) => {});
                }
                return burden;
            }
        }

        internal class ForcedScope : IDisposable
        {
            private readonly NetCoreScope previousScope;
            public ForcedScope(NetCoreScope scope)
            {
                previousScope = NetCoreScope.Current;
                NetCoreScope.current.Value = scope;
            }
            public void Dispose()
            {
                NetCoreScope.current.Value = previousScope;
            }
        }
    }
}