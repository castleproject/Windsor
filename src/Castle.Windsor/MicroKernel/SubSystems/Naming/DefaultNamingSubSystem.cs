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
		protected readonly Dictionary<Type, HandlerWithPriority> service2Handler =
			new Dictionary<Type, HandlerWithPriority>(SimpleTypeEqualityComparer.Instance);

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
					cache = new Dictionary<Type, IHandler>(service2Handler.Count, service2Handler.Comparer);
					foreach (var item in service2Handler)
					{
						cache.Add(item.Key, item.Value.Handler);
					}
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

			if (service.IsGenericType && service.IsGenericTypeDefinition == false)
			{
				var openService = service.GetGenericTypeDefinition();
				if (HandlerByServiceCache.TryGetValue(openService, out handler) && handler.Supports(service))
				{
					return handler;
				}

				var handlerCandidates = GetHandlers(openService);
				foreach (var handlerCandidate in handlerCandidates)
				{
					if (handlerCandidate.Supports(service))
					{
						return handlerCandidate;
					}
				}
			}

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
				result = GetHandlersNoLock(service);

				locker.Upgrade();
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
					var handlerForService = serviceSelector(service);
					HandlerWithPriority previous;
					if (service2Handler.TryGetValue(service, out previous) == false || handlerForService.Triumphs(previous))
					{
						service2Handler[service] = handlerForService;
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
				result = name2Handler.Values.Where(h => h.SupportsAssignable(service)).ToArray();
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

		private IHandler[] GetHandlersNoLock(Type service)
		{
			//we have 3 segments
			const int defaults = 0;
			const int regulars = 1;
			const int fallbacks = 2;
			var handlers = new SegmentedList<IHandler>(3);
			foreach (var handler in name2Handler.Values)
			{
				if (handler.Supports(service) == false)
				{
					continue;
				}
				if (IsDefault(handler, service))
				{
					handlers.AddFirst(defaults, handler);
					continue;
				}
				if (IsFallback(handler, service))
				{
					handlers.AddLast(fallbacks, handler);
					continue;
				}
				handlers.AddLast(regulars, handler);
			}
			return handlers.ToArray();
		}

		private Func<Type, HandlerWithPriority> GetServiceSelector(IHandler handler)
		{
			var defaultsFilter = handler.ComponentModel.GetDefaultComponentForServiceFilter();
			var fallbackFilter = handler.ComponentModel.GetFallbackComponentForServiceFilter();
			if (defaultsFilter == null)
			{
				if (fallbackFilter == null)
				{
					return service => new HandlerWithPriority(0, handler);
				}
				return service => new HandlerWithPriority(fallbackFilter(service) ? -1 : 0, handler);
			}
			if (fallbackFilter == null)
			{
				return service => new HandlerWithPriority(defaultsFilter(service) ? 1 : 0, handler);
			}
			return service => new HandlerWithPriority(defaultsFilter(service) ? 1 : (fallbackFilter(service) ? -1 : 0), handler);
		}

		private bool IsDefault(IHandler handler, Type service)
		{
			var filter = handler.ComponentModel.GetDefaultComponentForServiceFilter();
			if (filter == null)
			{
				return false;
			}
			return filter(service);
		}

		private bool IsFallback(IHandler handler, Type service)
		{
			var filter = handler.ComponentModel.GetFallbackComponentForServiceFilter();
			if (filter == null)
			{
				return false;
			}
			return filter(service);
		}

		protected struct HandlerWithPriority
		{
			private readonly IHandler handler;
			private readonly int priority;

			public HandlerWithPriority(int priority, IHandler handler)
			{
				this.priority = priority;
				this.handler = handler;
			}

			public IHandler Handler
			{
				get { return handler; }
			}

			public bool Triumphs(HandlerWithPriority other)
			{
				if (priority > other.priority)
				{
					return true;
				}
				if (priority == other.priority && priority > 0)
				{
					return true;
				}
				return false;
			}
		}
	}
}