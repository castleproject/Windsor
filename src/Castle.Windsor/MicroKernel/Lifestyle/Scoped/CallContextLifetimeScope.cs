// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
	using System.Diagnostics;
	using System.Globalization;
	using System;
	using System.Collections.Concurrent;
#if FEATURE_REMOTING
	using System.Runtime.Remoting.Messaging;
#endif
	using System.Security;
	using System.Threading;
	using Castle.Core;
	using Castle.Core.Internal;

	/// <summary>
	/// Provides explicit lifetime scoping within logical path of execution. Used for types with <see cref="LifestyleType.Scoped" />.
	/// </summary>
	/// <remarks>
	/// The scope is passed on to child threads, including ThreadPool threads. The capability is limited to a single AppDomain
	/// and should be used cautiously as calls to <see cref="Dispose" /> may occur while the child thread is still executing,
	/// which in turn may lead to subtle threading bugs.
	/// </remarks>
	public class CallContextLifetimeScope : ILifetimeScope
	{
		private static readonly ConcurrentDictionary<Guid, CallContextLifetimeScope> allScopes =
			new ConcurrentDictionary<Guid, CallContextLifetimeScope>();

#if FEATURE_REMOTING
		private static readonly string callContextKey = "castle.lifetime-scope-" + AppDomain.CurrentDomain.Id.ToString(CultureInfo.InvariantCulture);
#else
		private static readonly AsyncLocal<Guid> asyncLocal = new AsyncLocal<Guid>();
#endif

		private readonly Guid contextId = Guid.NewGuid();
		private readonly CallContextLifetimeScope parentScope;
		private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
		private ScopeCache cache = new ScopeCache();

		public CallContextLifetimeScope()
		{
			contextId = Guid.NewGuid();
			parentScope = ObtainCurrentScope();

			var added = allScopes.TryAdd(contextId, this);
			Debug.Assert(added);
			SetCurrentScope(this);
		}

		[SecuritySafeCritical]
		public void Dispose()
		{
			@lock.EnterUpgradeableReadLock();
			try
			{
				// Dispose the burden cache
				if (cache == null) return;
				@lock.EnterWriteLock();
				try
				{
					cache.Dispose();
					cache = null;

					// Restore the parent scope (if inside one)
					if (parentScope != null)
					{
						SetCurrentScope(parentScope);
					}
					else
					{
#if FEATURE_REMOTING
						CallContext.FreeNamedDataSlot(callContextKey);
#endif
					}
				}
				finally
				{
					@lock.ExitWriteLock();
				}
			}
			finally
			{
				@lock.ExitUpgradeableReadLock();
			}

			CallContextLifetimeScope @this;
			allScopes.TryRemove(contextId, out @this);
		}

		public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
		{
			@lock.EnterUpgradeableReadLock();
			try
			{
				var burden = cache[model];
				if (burden == null)
				{
					@lock.EnterWriteLock();
					try
					{
						burden = createInstance(delegate { });
						cache[model] = burden;
					}
					finally
					{
						@lock.ExitWriteLock();
					}
				}
				return burden;
			}
			finally
			{
				@lock.ExitUpgradeableReadLock();
			}
		}

		[SecuritySafeCritical]
		private static void SetCurrentScope(CallContextLifetimeScope lifetimeScope)
		{
#if FEATURE_REMOTING
			CallContext.LogicalSetData(callContextKey, lifetimeScope.contextId);
#else
			asyncLocal.Value = lifetimeScope.contextId;
#endif
		}

		[SecuritySafeCritical]
		public static CallContextLifetimeScope ObtainCurrentScope()
		{
#if FEATURE_REMOTING
			var scopeKey = CallContext.LogicalGetData(callContextKey);
#else
			var scopeKey = asyncLocal.Value;
#endif
			if (scopeKey == null)
			{
				return null;
			}

			CallContextLifetimeScope scope;
			allScopes.TryGetValue((Guid)scopeKey, out scope);
			return scope;
		}
	}
}