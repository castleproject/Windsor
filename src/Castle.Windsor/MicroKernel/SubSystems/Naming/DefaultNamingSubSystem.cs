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

namespace Castle.MicroKernel.SubSystems.Naming
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Util;

	[Serializable]
	public class DefaultNamingSubSystem : AbstractSubSystem, INamingSubSystem
	{
		protected readonly Lock @lock = Lock.Create();

		/// <summary>
		///   Map(String, IHandler) to map component names to <see cref="IHandler" /> Items in this dictionary are sorted in insertion order.
		/// </summary>
		protected readonly Dictionary<string, IHandler> name2Handler =
			new Dictionary<string, IHandler>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		///   Map(Type, IHandler) to map a service to <see cref="IHandler" /> . If there is more than a single service of the type, only the first registered services is stored in this dictionary. It serve as a fast lookup for the common case of having a single handler for a type.
		/// </summary>
		protected readonly Dictionary<Type, IHandler> service2Handler =
			new Dictionary<Type, IHandler>(SimpleTypeEqualityComparer.Instance);

		protected IList<IHandlersFilter> filters;
		protected IList<IHandlerSelector> selectors;

		private readonly IDictionary<Type, IHandler[]> assignableHandlerListsByTypeCache =
			new Dictionary<Type, IHandler[]>(SimpleTypeEqualityComparer.Instance);

		private readonly IDictionary<Type, IHandler[]> handlerListsByTypeCache =
			new Dictionary<Type, IHandler[]>(SimpleTypeEqualityComparer.Instance);

		private Dictionary<string, IHandler> handlerByNameCache;
		private Dictionary<Type, IHandler> handlerByServiceCache;

		public virtual int ComponentCount
		{
			get { return HandlerByNameCache.Count; }
		}

		protected IDictionary<string, IHandler> HandlerByNameCache
		{
			get
			{
				var cache = handlerByNameCache;
				if (cache != null)
				{
					return cache;
				}
				using (@lock.ForWriting())
				{
					cache = new Dictionary<string, IHandler>(name2Handler, name2Handler.Comparer);
					handlerByNameCache = cache;
					return cache;
				}
			}
		}

		protected IDictionary<Type, IHandler> HandlerByServiceCache
		{
			get
			{
				var cache = handlerByServiceCache;
				if (cache != null)
				{
					return cache;
				}
				using (@lock.ForWriting())
				{
					cache = new Dictionary<Type, IHandler>(service2Handler, service2Handler.Comparer);
					handlerByServiceCache = cache;
					return cache;
				}
			}
		}

		public void AddHandlerSelector(IHandlerSelector selector)
		{
			if (selectors == null)
			{
				selectors = new List<IHandlerSelector>();
			}
			selectors.Add(selector);
		}

		public void AddHandlersFilter(IHandlersFilter filter)
		{
			if (filters == null)
			{
				filters = new List<IHandlersFilter>();
			}
			filters.Add(filter);
		}

		public virtual bool Contains(String name)
		{
			return HandlerByNameCache.ContainsKey(name);
		}

		public virtual bool Contains(Type service)
		{
			return GetHandler(service) != null;
		}

		public virtual IHandler[] GetAllHandlers()
		{
			var cache = HandlerByNameCache;
			var list = new IHandler[cache.Values.Count];
			cache.Values.CopyTo(list, 0);
			return list;
		}

		public virtual IHandler[] GetAssignableHandlers(Type service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (service == typeof(object))
			{
				return GetAllHandlers();
			}
			return GetAssignableHandlersNoFiltering(service);
		}

		public virtual IHandler GetHandler(String name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (selectors != null)
			{
				var selectorsOpinion = GetSelectorsOpinion(name, null);
				if (selectorsOpinion != null)
				{
					return selectorsOpinion;
				}
			}

			IHandler value;
			HandlerByNameCache.TryGetValue(name, out value);
			return value;
		}

		public virtual IHandler GetHandler(Type service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (selectors != null)
			{
				var selectorsOpinion = GetSelectorsOpinion(null, service);
				if (selectorsOpinion != null)
				{
					return selectorsOpinion;
				}
			}
			IHandler handler;
			if (HandlerByServiceCache.TryGetValue(service, out handler))
			{
				return handler;
			}

			if (service.IsGenericType &&
			    HandlerByServiceCache.TryGetValue(service.GetGenericTypeDefinition(), out handler) &&
			    handler.Supports(service))
			{
				return handler;
			}
			// NOTE: at this point we might want to interrogate other handlers
			// that support the open version of the service...

			return null;
		}

		public virtual IHandler[] GetHandlers(Type service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (filters != null)
			{
				var filtersOpinion = GetFiltersOpinion(service);
				if (filtersOpinion != null)
				{
					return filtersOpinion;
				}
			}

			IHandler[] result;
			using (var locker = @lock.ForReadingUpgradeable())
			{
				if (handlerListsByTypeCache.TryGetValue(service, out result))
				{
					return result;
				}

				locker.Upgrade();
				if (handlerListsByTypeCache.TryGetValue(service, out result))
				{
					return result;
				}

				result = name2Handler.Values.Where(h => h.Supports(service)).ToArray();
				handlerListsByTypeCache[service] = result;
			}

			return result;
		}

		public virtual void Register(IHandler handler)
		{
			var name = handler.ComponentModel.Name;
			using (@lock.ForWriting())
			{
				try
				{
					name2Handler.Add(name, handler);
				}
				catch (ArgumentException)
				{
					throw new ComponentRegistrationException(
						String.Format(
							"Component {0} could not be registered. There is already a component with that name. Did you want to modify the existing component instead? If not, make sure you specify a unique name.",
							name));
				}
				var serviceSelector = GetServiceSelector(handler);
				foreach (var service in handler.ComponentModel.Services)
				{
					if (serviceSelector(service))
					{
						service2Handler[service] = handler;
					}
				}
				InvalidateCache();
			}
		}

		protected IHandler[] GetAssignableHandlersNoFiltering(Type service)
		{
			IHandler[] result;
			using (var locker = @lock.ForReadingUpgradeable())
			{
				if (assignableHandlerListsByTypeCache.TryGetValue(service, out result))
				{
					return result;
				}

				locker.Upgrade();
				if (assignableHandlerListsByTypeCache.TryGetValue(service, out result))
				{
					return result;
				}

				var handlers = name2Handler.Values;
				var services = new List<IHandler>();
				foreach (var handler in handlers)
				{
					if (handler.ComponentModel.Services.Any(handlerService => IsAssignable(service, handlerService)))
					{
						services.Add(handler);
					}
				}
				result = services.ToArray();
				assignableHandlerListsByTypeCache[service] = result;
			}

			return result;
		}

		protected virtual IHandler[] GetFiltersOpinion(Type service)
		{
			if (filters == null)
			{
				return null;
			}

			IHandler[] handlers = null;
			foreach (var filter in filters)
			{
				if (filter.HasOpinionAbout(service) == false)
				{
					continue;
				}
				if (handlers == null)
				{
					handlers = GetAssignableHandlersNoFiltering(service);
				}
				handlers = filter.SelectHandlers(service, handlers);
				if (handlers != null)
				{
					return handlers;
				}
			}
			return null;
		}

		protected virtual IHandler GetSelectorsOpinion(string name, Type type)
		{
			if (selectors == null)
			{
				return null;
			}
			type = type ?? typeof(object); // if type is null, we want everything, so object does well for that
			IHandler[] handlers = null; //only init if we have a selector with an opinion about this type
			foreach (var selector in selectors)
			{
				if (selector.HasOpinionAbout(name, type) == false)
				{
					continue;
				}
				if (handlers == null)
				{
					handlers = GetAssignableHandlersNoFiltering(type);
				}
				var handler = selector.SelectHandler(name, type, handlers);
				if (handler != null)
				{
					return handler;
				}
			}
			return null;
		}

		protected void InvalidateCache()
		{
			handlerListsByTypeCache.Clear();
			assignableHandlerListsByTypeCache.Clear();
			handlerByNameCache = null;
			handlerByServiceCache = null;
		}

		[DebuggerHidden] // prevents the debugger from braking in the try catch block when debugger option 'thrown' is set
		protected bool IsAssignable(Type thisOne, Type fromThisOne)
		{
			if (thisOne.IsAssignableFrom(fromThisOne))
			{
				return true;
			}
			if (thisOne.IsGenericType == false || fromThisOne.IsGenericTypeDefinition == false)
			{
				return false;
			}
			var genericArguments = thisOne.GetGenericArguments();
			if (fromThisOne.GetGenericArguments().Length != genericArguments.Length)
			{
				return false;
			}
			try
			{
				fromThisOne = fromThisOne.MakeGenericType(genericArguments);
			}
			catch (ArgumentException)
			{
				// Any element of typeArguments does not satisfy the constraints specified for the corresponding type parameter of the current generic type.
				// NOTE: We try and catch because there's no API to reliably, and robustly test for that upfront
				return false;
			}
			return thisOne.IsAssignableFrom(fromThisOne);
		}

		private Predicate<Type> GetServiceSelector(IHandler handler)
		{
			var customFilter =
				(Predicate<Type>) handler.ComponentModel.ExtendedProperties[Constants.DefaultComponentForServiceFilter];
			if (customFilter == null)
			{
				return service => service2Handler.ContainsKey(service) == false;
			}
			return service => service2Handler.ContainsKey(service) == false || customFilter(service);
		}
	}
}