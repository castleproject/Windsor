// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System;
#if DOTNET35
	using System.Collections.Generic;
#else
	using System.Collections.Concurrent;
#endif
	using System.Runtime.Remoting.Messaging;
	using System.Runtime.Serialization;
	using System.Security;
	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.Windsor;

	/// <summary>
	///   Provides explicit lifetime scoping within logical path of execution. Used for types with <see
	///    cref="LifestyleType.Scoped" /> .
	/// </summary>
	/// <remarks>
	///   The scope is passed on to child threads, including ThreadPool threads. The capability is limited to single <see
	///    cref="AppDomain" /> and should be used cauciously as call to <see cref="Dispose" /> may occur while the child thread is still executing, what in turn may lead to subtle threading bugs.
	/// </remarks>
	[Serializable]
	public class CallContextLifetimeScope : ILifetimeScope, ISerializable
	{
#if DOTNET35
		private static readonly object locker = new object();
		private static readonly Dictionary<Guid, CallContextLifetimeScope> appDomainLocalInstanceCache =
			new Dictionary<Guid, CallContextLifetimeScope>();
#else
		private static readonly ConcurrentDictionary<Guid, CallContextLifetimeScope> appDomainLocalInstanceCache =
			new ConcurrentDictionary<Guid, CallContextLifetimeScope>();
#endif

		private static readonly string contextKey = "castle.lifetime-scope-" + AppDomain.CurrentDomain.Id.ToString();
		private readonly Guid instanceId;
		private readonly Lock @lock = Lock.Create();
		private readonly CallContextLifetimeScope parentScope;
		private ScopeCache cache = new ScopeCache();

		public CallContextLifetimeScope(IKernel container)
		{
			var parent = ObtainCurrentScope();
			if (parent != null)
			{
				parentScope = parent;
			}
			SetCurrentScope(this);
			instanceId = Guid.NewGuid();
		}

		public CallContextLifetimeScope(IWindsorContainer container) : this(container.Kernel)
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
				else
				{
					CallContext.FreeNamedDataSlot(contextKey);
				}
			}
#if DOTNET35
			lock (locker)
			{
				appDomainLocalInstanceCache.Remove(instanceId);
			}
#else
			CallContextLifetimeScope @this;
			appDomainLocalInstanceCache.TryRemove(instanceId, out @this);
#endif

		}

		public Burden GetCachedInstance(ComponentModel instance, ScopedInstanceActivationCallback createInstance)
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				var burden = cache[instance];
				if (burden == null)
				{
					token.Upgrade();

					burden = createInstance(delegate { });
					cache[instance] = burden;
				}
				return burden;
			}
		}
		
#if !CLIENTPROFILE
		[SecuritySafeCritical]
#endif
		private void SetCurrentScope(CallContextLifetimeScope lifetimeScope)
		{
			CallContext.LogicalSetData(contextKey, lifetimeScope);
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(typeof (SerializationReference));
			info.AddValue("instanceId", instanceId);
#if DOTNET35
			lock (locker)
			{
				if(appDomainLocalInstanceCache.ContainsKey(instanceId) == false)
				{
					appDomainLocalInstanceCache.Add(instanceId, this);
				}
			}
#else
			appDomainLocalInstanceCache.TryAdd(instanceId, this);
#endif
		}

#if !CLIENTPROFILE
		[SecuritySafeCritical]
#endif
		public static CallContextLifetimeScope ObtainCurrentScope()
		{
			return (CallContextLifetimeScope) CallContext.LogicalGetData(contextKey);
		}

		[Serializable]
		private class CrossAppDomainCallContextLifetimeScope : ILifetimeScope, ISerializable
		{
			private readonly Guid originalInstanceId;

			public CrossAppDomainCallContextLifetimeScope(Guid originalInstanceId)
			{
				this.originalInstanceId = originalInstanceId;
			}

			[SecurityCritical]
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.SetType(typeof (SerializationReference));
				info.AddValue("instanceId", originalInstanceId);
			}

			void IDisposable.Dispose()
			{
			}

			Burden ILifetimeScope.GetCachedInstance(ComponentModel instance, ScopedInstanceActivationCallback createInstance)
			{
				// not sure if we'll be able to hit this code ever. If so we should get a better exception message
				throw new InvalidOperationException(
					"This scope comes from a different app domain and cannot be used in this context.");
			}
		}

		[Serializable]
		private class SerializationReference : IObjectReference, ISerializable
		{
			private readonly Guid scopeInstanceId;

			[SecurityCritical]
			protected SerializationReference(SerializationInfo info, StreamingContext context)
			{
				scopeInstanceId = (Guid) info.GetValue("instanceId", typeof (Guid));
			}


			[SecurityCritical]
			object IObjectReference.GetRealObject(StreamingContext context)
			{
				CallContextLifetimeScope scope;
#if DOTNET35
				lock (locker)
				{
					appDomainLocalInstanceCache.TryGetValue(scopeInstanceId, out scope);
				}
#else
				appDomainLocalInstanceCache.TryGetValue(scopeInstanceId, out scope);
#endif
				appDomainLocalInstanceCache.TryGetValue(scopeInstanceId, out scope);
				return (object) scope ?? new CrossAppDomainCallContextLifetimeScope(scopeInstanceId);
			}

			[SecurityCritical]
			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
		}
	}
#endif
}