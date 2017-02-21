// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Lifestyle.Scoped
{
#if !SILVERLIGHT
    using System.Diagnostics;
    using System.Globalization;
    using System;
#if DOTNET35
	using System.Collections.Generic;
#else
    using System.Collections.Concurrent;
#endif

#if FEATURE_REMOTING
	using System.Runtime.Remoting.Messaging;
#endif
    using System.Threading;
    using System.Security;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.Windsor;

    /// <summary>
    ///   Provides explicit lifetime scoping within logical path of execution. Used for types with <see
    ///    cref="LifestyleType.Scoped" /> .
    /// </summary>
    /// <remarks>
    ///   The scope is passed on to child threads, including ThreadPool threads. The capability is limited to single
    ///   AppDomain and should be used cautiously as call to <see cref="Dispose" /> may occur while the child thread is still executing, what in turn may lead to subtle threading bugs.
    /// </remarks>
    public class CallContextLifetimeScope : ILifetimeScope
    {
#if DOTNET35
		private static readonly object cacheLocker = new object();
		private static readonly Dictionary<Guid, CallContextLifetimeScope> appDomainLocalInstanceCache =
			new Dictionary<Guid, CallContextLifetimeScope>();
#else
        private static readonly ConcurrentDictionary<Guid, CallContextLifetimeScope> appDomainLocalInstanceCache =
            new ConcurrentDictionary<Guid, CallContextLifetimeScope>();
#endif

#if FEATURE_REMOTING
        private static readonly string keyInCallContext = "castle.lifetime-scope-" +
#if FEATURE_APPDOMAIN
            AppDomain.CurrentDomain.Id.ToString(CultureInfo.InvariantCulture);
#else
            "0";
#endif

#else
        private static readonly AsyncLocal<Guid> CallContextData = new AsyncLocal<Guid>();
#endif
        private readonly Guid contextId;
        private readonly Lock @lock = Lock.Create();
        private readonly CallContextLifetimeScope parentScope;
        private ScopeCache cache = new ScopeCache();

        public CallContextLifetimeScope(IKernel container) : this()
        {

        }

        public CallContextLifetimeScope()
        {
            var parent = ObtainCurrentScope();
            if (parent != null)
            {
                parentScope = parent;
            }
            contextId = Guid.NewGuid();
#if DOTNET35
			lock(cacheLocker)
			{
				appDomainLocalInstanceCache.Add(contextId, this);
			}
#else
            var added = appDomainLocalInstanceCache.TryAdd(contextId, this);
            Debug.Assert(added);
#endif
            SetCurrentScope(this);
        }

        public CallContextLifetimeScope(IWindsorContainer container) : this()
        {
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            using (var token = @lock.ForReadingUpgradeable())
            {
                if (cache == null)
                {
                    return;
                }
                token.Upgrade();
                cache.Dispose();
                cache = null;

                if (parentScope != null)
                {
                    SetCurrentScope(parentScope);
                }
#if FEATURE_REMOTING
                else
				{
					CallContext.FreeNamedDataSlot(keyInCallContext);
				}
#endif
            }
#if DOTNET35
			lock (cacheLocker)
			{
				appDomainLocalInstanceCache.Remove(contextId);
			}
#else
            CallContextLifetimeScope @this;
            appDomainLocalInstanceCache.TryRemove(contextId, out @this);
#endif

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

#if !CLIENTPROFILE
        [SecuritySafeCritical]
#endif
		private void SetCurrentScope(CallContextLifetimeScope lifetimeScope)
		{
#if FEATURE_REMOTING
            CallContext.LogicalSetData(keyInCallContext, lifetimeScope.contextId);
#else
            CallContextData.Value = lifetimeScope.contextId;
#endif
        }

#if !CLIENTPROFILE
		[SecuritySafeCritical]
#endif
		public static CallContextLifetimeScope ObtainCurrentScope()
		{
#if FEATURE_REMOTING
			var scopeKey = CallContext.LogicalGetData(keyInCallContext);
#else
            var scopeKey = CallContextData.Value;
#endif
            if (scopeKey == null)
			{
				return null;
			}
			CallContextLifetimeScope scope;
			appDomainLocalInstanceCache.TryGetValue((Guid)scopeKey, out scope);
			return scope;
		}
	}
#endif
}