﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration.Internal
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;
	using System.Threading;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	internal static class WcfUtils
	{
		public static readonly Arguments EmptyArguments = new Arguments();

		public static bool IsHosted(IWcfServiceModel serviceModel)
		{
			return serviceModel.IsHosted;
		}

		public static bool IsServiceContract(Type service)
		{
			if (service.IsDefined(typeof(ServiceContractAttribute), true))
			{
				return true;
			}

			if (service.IsInterface)
			{
				return service.GetInterfaces().Any(parent => IsServiceContract(parent));
			}

			return false;
		}

		public static WcfExtensionScope GetScope(ComponentModel model)
		{
			if (model.Configuration != null)
			{
				var scopeAttrib = model.Configuration.Attributes[WcfConstants.ExtensionScopeKey];
				if (!string.IsNullOrEmpty(scopeAttrib))
				{
					switch (scopeAttrib.ToLower())
					{
						case "clients":
							return WcfExtensionScope.Clients;
						case "services":
							return WcfExtensionScope.Services;
						case "explicit":
							return WcfExtensionScope.Explicit;
						default:
							const string message = @"The attribute 'scope' must be 'clients', 'services' or 'explicit'";
							throw new ConfigurationErrorsException(message);
					}
				}
			}
			return WcfExtensionScope.Undefined;
		}

		internal static void AddBehaviors<T>(IKernel kernel, WcfExtensionScope scope,
										   KeyedByTypeCollection<T> behaviors, IWcfBurden burden)
		{
			AddBehaviors(kernel, scope, behaviors, burden, null);
		}

		internal static void AddBehaviors<T>(IKernel kernel, WcfExtensionScope scope,
										   KeyedByTypeCollection<T> behaviors, IWcfBurden burden,
										   Predicate<T> predicate)
		{
			foreach (var handler in FindExtensions<T>(kernel, scope))
			{
				T behavior = (T)handler.Resolve(CreationContext.CreateEmpty());
				if (predicate == null || predicate(behavior))
				{
					if (behaviors != null) behaviors.Add(behavior);
					if (burden != null) burden.Add(behavior);
				}
			}
		}

		public static void ExtendBehavior(IKernel kernel, WcfExtensionScope scope, object behavior, CreationContext context)
		{
			var behaviorType = behavior.GetType();
			if (IsExtensibleOfItself(behaviorType) == false)
			{
				return;
			}
			
			var extensionType = typeof(IExtension<>).MakeGenericType(behaviorType);
			foreach (var extensionHandler in FindExtensions(kernel, scope, extensionType))
			{
				var extension = extensionHandler.Resolve(context);
				AttachExtension(behavior, extension);
			}
		}

		public static IEnumerable<IHandler> FindExtensions<T>(IKernel kernel, WcfExtensionScope scope)
		{
			return FindExtensions(kernel, scope, typeof(T));
		}

		public static IEnumerable<IHandler> FindExtensions(IKernel kernel, WcfExtensionScope scope, Type type)
		{
			foreach (var handler in kernel.GetAssignableHandlers(type))
			{
				var model = handler.ComponentModel;
				var extensionsScope = GetScope(model);
				if (ScopesCompatible(scope, extensionsScope))
				{
					yield return handler;
				}
			}
		}

		public static void AddExtensionDependencies<T>(IKernel kernel, WcfExtensionScope scope, ComponentModel model)
		{
			foreach (var handler in FindExtensions<T>(kernel, scope))
			{
				AddExtensionDependency(null, handler.ComponentModel.Services.Single(), model);
			}
		}

		public static void AddExtensionDependency(string dependencyKey, Type serviceType, ComponentModel model)
		{
			model.Dependencies.Add(new ComponentDependencyModel(dependencyKey, serviceType));
		}

		public static bool IsExtension<T>(object extension)
		{
			var owner = typeof(T);
			return IsExtension(extension, ref owner);
		}

		public static bool IsExtension(object extension, ref Type owner)
		{
			if (extension != null)
			{
				var extensionType = extension.GetType();
				var extensionInterface = extensionType.GetInterface(typeof(IExtension<>).FullName);

				if (extensionInterface != null)
				{
					var args = extensionInterface.GetGenericArguments();

					if (args.Length == 1)
					{
						if (owner == null || owner == args[0])
						{
							owner = args[0];
							return true;
						}
					}
				}
			}
			return false;
		}

		public static bool AttachExtension(object owner, object extension)
		{
			var helper = (IWcfExtensionHelper)
							Activator.CreateInstance(typeof(WcfExtensionHelper<>)
								.MakeGenericType(owner.GetType()), owner);
			return helper.AddExtension(extension);
		}

		public static bool AttachExtension<T>(KeyedByTypeCollection<T> candidates, object extension)
		{
			Type owner = null;
			if (IsExtension(extension, ref owner))
			{
				return AttachExtension(candidates, extension, owner);
			}
			return false;
		}

		public static bool AttachExtension<T>(KeyedByTypeCollection<T> candidates, object extension, Type owner)
		{
			if (typeof(T).IsAssignableFrom(owner))
			{
				var helper = (IWcfExtensionHelper)Activator.CreateInstance(typeof(WcfExtensionHelper<,>)
					.MakeGenericType(typeof(T), owner), candidates);
				return helper.AddExtension(extension);
			}
			return false;
		}

		public static void BindServiceHostAware(ServiceHost serviceHost, IServiceHostAware serviceHostAware, bool created)
		{
			if (created) serviceHostAware.Created(serviceHost);
			serviceHost.Opening += delegate { serviceHostAware.Opening(serviceHost); };
			serviceHost.Opened += delegate { serviceHostAware.Opened(serviceHost); };
			serviceHost.Closing += delegate { serviceHostAware.Closing(serviceHost); };
			serviceHost.Closed += delegate { serviceHostAware.Closed(serviceHost); };
			serviceHost.Faulted += delegate { serviceHostAware.Faulted(serviceHost); };
		}

		public static void BindChannelFactoryAware(ChannelFactory channelFactory, IChannelFactoryAware channelFactoryAware, bool created)
		{
			if (created) channelFactoryAware.Created(channelFactory);
			channelFactory.Opening += delegate { channelFactoryAware.Opening(channelFactory); };
			channelFactory.Opened += delegate { channelFactoryAware.Opened(channelFactory); };
			channelFactory.Closing += delegate { channelFactoryAware.Closing(channelFactory); };
			channelFactory.Closed += delegate { channelFactoryAware.Closed(channelFactory); };
			channelFactory.Faulted += delegate { channelFactoryAware.Faulted(channelFactory); };
		}

		public static bool RegisterErrorHandler(ServiceHost serviceHost, IErrorHandler errorHandler, bool skipPiggybacks)
		{
			if (skipPiggybacks && IsBehaviorExtension(errorHandler)) return false;
			serviceHost.Description.Behaviors.Add(new WcfErrorBehavior(errorHandler));
			return true;
		}

		public static bool RegisterErrorHandler(ServiceEndpoint endpoint, IErrorHandler errorHandler, bool skipPiggybacks)
		{
			if (skipPiggybacks && IsBehaviorExtension(errorHandler)) return false;
			endpoint.Behaviors.Add(new WcfErrorBehavior(errorHandler));
			return true;
		}

		public static bool IsBehaviorExtension(object behavior)
		{
			return behavior is IServiceBehavior || behavior is IContractBehavior ||
				   behavior is IOperationBehavior || behavior is IEndpointBehavior;
		}

		public static IEnumerable<T> FindDependencies<T>(IDictionary dependencies)
		{
			return FindDependencies<T>(dependencies, null);
		}

		public static IEnumerable<T> FindDependencies<T>(IDictionary dependencies, Predicate<T> test)
		{
			if (dependencies != null)
			{
				foreach (object dependency in dependencies.Values)
				{
					if (dependency is T)
					{
						T candidate = (T)dependency;

						if (test == null || test(candidate))
						{
							yield return candidate;
						}
					}
					else if (dependency is IEnumerable<T>)
					{
						foreach (T item in (IEnumerable<T>)dependency)
						{
							yield return item;
						}
					}
				}
			}
		}

		public static bool IsCommunicationObjectReady(ICommunicationObject comm)
		{
			switch (comm.State)
			{
				case CommunicationState.Closed:
				case CommunicationState.Closing:
				case CommunicationState.Faulted:
					return false;
			}
			return true;
		}

		public static void ReleaseCommunicationObject(ICommunicationObject comm, TimeSpan? timeout)
		{
			if (comm != null)
			{
				if (comm.State != CommunicationState.Faulted)
				{
					try
					{
						if (timeout.HasValue)
						{
							comm.Close(timeout.Value);
						}
						else
						{
							comm.Close();
						}
					}
					catch
					{
						comm.Abort();
					}
				}
				else
				{
					comm.Abort();
				}
			}
		}

		public static ICollection<T> AddAll<T>(this ICollection<T> collection, params T[] items)
		{
			return AddAll(collection, (IEnumerable<T>)items);
		}

		public static ICollection<T> AddAll<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				collection.Add(item);
			}
			return collection;
		}

		public static Type GetClosedGenericDefinition(Type openGenericDefinition, Type implementation)
		{
			var genericArgument =
				(from @interface in implementation.GetInterfaces()
				 where @interface.IsGenericType && @interface.GetGenericTypeDefinition() == openGenericDefinition
				 select @interface.GetGenericArguments()[0]
				 ).FirstOrDefault();

			return (genericArgument != null) ? openGenericDefinition.MakeGenericType(genericArgument) : null;
		}

		public static T SafeInitialize<T>(ref T cache, Func<T> source) where T : class
		{
			T getCache = Interlocked.CompareExchange(ref cache, null, null);
			if (getCache != null) return getCache;

			getCache = source();
			T updatedCache = Interlocked.CompareExchange(ref cache, getCache, null);
			return updatedCache ?? getCache;
		}

		/// <summary>
		/// Checks if given <paramref name="type"/> <c>Foo</c> implements <c>IExtensibleObject&lt;Foo&gt;</c>
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static bool IsExtensibleOfItself(Type type)
		{
			var extensibleType = type.GetInterface(typeof(IExtensibleObject<>).FullName);
			if (extensibleType == null)
			{
				return false;
			}
			var args = extensibleType.GetGenericArguments();
			if (args.Length != 1 || args[0] != type)
			{
				return false;
			}
			return true;
		}

		private static bool ScopesCompatible(WcfExtensionScope extendeeScope, WcfExtensionScope extensionScope)
		{
			if ((extensionScope != WcfExtensionScope.Explicit || extendeeScope == WcfExtensionScope.Explicit))
			{
				return (extendeeScope == extensionScope || extendeeScope == WcfExtensionScope.Undefined ||
						extensionScope == WcfExtensionScope.Undefined);
			}
			return false;
		}
	}
}
