// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
	using System.Linq;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Util;

	/// <summary>
	///   Default <see cref = "INamingSubSystem" /> implementation.
	///   Keeps services map as a simple hash table.
	///   Keeps key map as a list dictionary to maintain order.
	///   Does not support a query string.
	/// </summary>
	[Serializable]
	public class DefaultNamingSubSystem : AbstractSubSystem, INamingSubSystem, IExposeDefaultComponentsForServices
	{
		/// <summary>
		///   Map(String, IHandler) to map component keys
		///   to <see cref = "IHandler" />
		///   Items in this dictionary are sorted in insertion order.
		/// </summary>
		protected readonly Dictionary<string, IHandler> key2Handler = new Dictionary<string, IHandler>(StringComparer.OrdinalIgnoreCase);

		protected readonly Lock @lock = Lock.Create();

		/// <summary>
		///   Map(Type, IHandler) to map a service
		///   to <see cref = "IHandler" />.
		///   If there is more than a single service of the type, only the first
		///   registered services is stored in this dictionary.
		///   It serve as a fast lookup for the common case of having a single handler for 
		///   a type.
		/// </summary>
		protected readonly Dictionary<Type, IHandler> service2Handler = new Dictionary<Type, IHandler>(SimpleTypeEqualityComparer.Instance);

		protected IList<IHandlersFilter> filters;
		protected IList<IHandlerSelector> selectors;

		private readonly IDictionary<Type, IHandler[]> assignableHandlerListsByTypeCache = new Dictionary<Type, IHandler[]>(SimpleTypeEqualityComparer.Instance);
		private readonly IDictionary<Type, IHandler[]> handlerListsByTypeCache = new Dictionary<Type, IHandler[]>(SimpleTypeEqualityComparer.Instance);
		private Dictionary<string, IHandler> key2HandlerCache;
		private Dictionary<Type, IHandler> service2HandlerCache;

		public virtual int ComponentCount
		{
			get { return HandlerByKeyCache.Count; }
		}

		protected IDictionary<string, IHandler> HandlerByKeyCache
		{
			get
			{
				var cache = key2HandlerCache;
				if (cache != null)
				{
					return cache;
				}
				using (@lock.ForWriting())
				{
					cache = new Dictionary<string, IHandler>(key2Handler, key2Handler.Comparer);
					key2HandlerCache = cache;
					return cache;
				}
			}
		}

		protected IDictionary<Type, IHandler> HandlerByServiceCache
		{
			get
			{
				var cache = service2HandlerCache;
				if (cache != null)
				{
					return cache;
				}
				using (@lock.ForWriting())
				{
					cache = new Dictionary<Type, IHandler>(service2Handler, service2Handler.Comparer);
					service2HandlerCache = cache;
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

		public virtual bool Contains(String key)
		{
			return HandlerByKeyCache.ContainsKey(key);
		}

		public virtual bool Contains(Type service)
		{
			return HandlerByServiceCache.ContainsKey(service);
		}

		public virtual IHandler[] GetAllHandlers()
		{
			var cache = HandlerByKeyCache;
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

		public virtual IHandler GetHandler(String key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (selectors != null)
			{
				var selectorsOpinion = GetSelectorsOpinion(key, null);
				if (selectorsOpinion != null)
				{
					return selectorsOpinion;
				}
			}

			IHandler value;
			HandlerByKeyCache.TryGetValue(key, out value);
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
			HandlerByServiceCache.TryGetValue(service, out handler);
			return handler;
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

				var tempResult = key2Handler.Values.Where(h => h.Services.Any(s => s == service));
				if (service.IsGenericType && service.IsGenericTypeDefinition == false)
				{
					var openService = service.GetGenericTypeDefinition();
					tempResult = tempResult.Union(key2Handler.Values.Where(h => h.Services.Any(s => s == openService)));
				}
				result = tempResult.ToArray();
				handlerListsByTypeCache[service] = result;
			}

			return result;
		}

		public virtual void Register(IHandler handler)
		{
			var key = handler.ComponentModel.Name;
			using (@lock.ForWriting())
			{
				if (key2Handler.ContainsKey(key))
				{
					throw new ComponentRegistrationException(
						String.Format("There is a component already registered for the given name {0}", key));
				}
				key2Handler.Add(key, handler);
				var serviceSelector = GetServiceSelector(handler);
				foreach (var service in handler.Services)
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

				var handlers = key2Handler.Values;
				var services = new List<IHandler>();
				foreach (var handler in handlers)
				{
					if (handler.Services.Any(handlerService => IsAssignable(service, handlerService)))
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
				if (handlers != null && handlers.Length > 0)
				{
					return handlers;
				}
			}
			return null;
		}

		protected virtual IHandler GetSelectorsOpinion(string key, Type type)
		{
			if (selectors == null)
			{
				return null;
			}
			type = type ?? typeof(object); // if type is null, we want everything, so object does well for that
			IHandler[] handlers = null; //only init if we have a selector with an opinion about this type
			foreach (var selector in selectors)
			{
				if (selector.HasOpinionAbout(key, type) == false)
				{
					continue;
				}
				if (handlers == null)
				{
					handlers = GetAssignableHandlersNoFiltering(type);
				}
				var handler = selector.SelectHandler(key, type, handlers);
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
			key2HandlerCache = null;
			service2HandlerCache = null;
		}

		protected bool IsAssignable(Type thisOne, Type fromThisOne)
		{
			if(thisOne.IsAssignableFrom(fromThisOne))
			{
				return true;
			}
			if (thisOne.IsGenericType == false || fromThisOne.IsGenericTypeDefinition == false)
			{
				return false;
			}
			var genericArguments = thisOne.GetGenericArguments();
			if(fromThisOne.GetGenericArguments().Length!=genericArguments.Length)
			{
				return false;
			}
			fromThisOne = fromThisOne.MakeGenericType(genericArguments);
			return thisOne.IsAssignableFrom(fromThisOne);
		}

		private Predicate<Type> GetServiceSelector(IHandler handler)
		{
			var customFilter = (Predicate<Type>)handler.ComponentModel.ExtendedProperties[Constants.DefaultComponentForServiceFilter];
			if (customFilter == null)
			{
				return service => service2Handler.ContainsKey(service) == false;
			}
			return service => service2Handler.ContainsKey(service) == false || customFilter(service);
		}

		IEnumerable<KeyValuePair<Type, IHandler>> IExposeDefaultComponentsForServices.GetDefaultComponentsForServices()
		{
			return HandlerByServiceCache;
		}
	}
}