// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

using System.Collections.Generic;

namespace Castle.MicroKernel.SubSystems.Naming
{
	using System;
	using System.Threading;

	using Castle.Core.Internal;

	/// <summary>
	/// Default <see cref="INamingSubSystem"/> implementation.
	/// Keeps services map as a simple hash table.
	/// Keeps key map as a list dictionary to maintain order.
	/// Does not support a query string.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class DefaultNamingSubSystem : AbstractSubSystem, INamingSubSystem
	{
		/// <summary>
		/// Map(String, IHandler) to map component keys
		/// to <see cref="IHandler"/>
		/// Items in this dictionary are sorted in insertion order.
		/// </summary>
		protected readonly IDictionary<string, IHandler> key2Handler = new Dictionary<string, IHandler>();

		/// <summary>
		/// Map(Type, IHandler) to map a service
		/// to <see cref="IHandler"/>.
		/// If there is more than a single service of the type, only the first
		/// registered services is stored in this dictionary.
		/// It serve as a fast lookup for the common case of having a single handler for 
		/// a type.
		/// </summary>
		protected readonly IDictionary<Type, IHandler> service2Handler = new Dictionary<Type, IHandler>();

		private readonly IDictionary<Type, IHandler[]> handlerListsByTypeCache = new Dictionary<Type, IHandler[]>();
		private readonly IDictionary<Type, IHandler[]> assignableHandlerListsByTypeCache = new Dictionary<Type, IHandler[]>();
		private IHandler[] allHandlersCache;
		private readonly List<IHandler> allHandlers = new List<IHandler>();
		private readonly Lock @lock = Lock.Create();
		private readonly IList<IHandlerSelector> selectors = new List<IHandlerSelector>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultNamingSubSystem"/> class.
		/// </summary>
		public DefaultNamingSubSystem()
		{
		}

		#region INamingSubSystem Members

		public virtual void Register(String key, IHandler handler)
		{
			Type service = handler.Service;

			using(@lock.ForWriting())
			{
				if (key2Handler.ContainsKey(key))
				{
					throw new ComponentRegistrationException(
						String.Format("There is a component already registered for the given key {0}", key));
				}

				if (!service2Handler.ContainsKey(service))
				{
					this[service] = handler;
				}

				this[key] = handler;
				InvalidateCache();
			}
		}

		private void InvalidateCache()
		{
			handlerListsByTypeCache.Clear();
			assignableHandlerListsByTypeCache.Clear();
			allHandlersCache = null;
		}

		public virtual bool Contains(String key)
		{
			using (@lock.ForReading())
			{
				return key2Handler.ContainsKey(key);
			}
		}

		public virtual bool Contains(Type service)
		{
			using (@lock.ForReading())
			{
				return service2Handler.ContainsKey(service);
			}
		}

		public virtual void UnRegister(String key)
		{
			using (@lock.ForWriting())
			{
				IHandler value;
				if (key2Handler.TryGetValue(key, out value))
					allHandlers.Remove(value);
				key2Handler.Remove(key);
				InvalidateCache();
			}
		}

		public virtual void UnRegister(Type service)
		{
			using (@lock.ForWriting())
			{
				service2Handler.Remove(service);
				InvalidateCache();
			}
		}

		public virtual int ComponentCount
		{
			get
			{
				using (@lock.ForReading())
				{
					return allHandlers.Count;
				}
			}
		}

		public virtual IHandler GetHandler(String key)
		{
			if (key == null) throw new ArgumentNullException("key");

			IHandler selectorsOpinion = GetSelectorsOpinion(key, null);
			if (selectorsOpinion != null)
				return selectorsOpinion;

			using (@lock.ForReading())
			{
				IHandler value;
				key2Handler.TryGetValue(key, out value);
				return value;
			}
		}

		public virtual IHandler[] GetHandlers(String query)
		{
			throw new NotImplementedException();
		}

		public virtual IHandler GetHandler(Type service)
		{
			if (service == null) throw new ArgumentNullException("service");

			IHandler selectorsOpinion = GetSelectorsOpinion(null, service);
			if (selectorsOpinion != null)
				return selectorsOpinion;


			using (@lock.ForReading())
			{
				IHandler handler;

				service2Handler.TryGetValue(service, out handler);

				return handler;
			}
		}

		public virtual IHandler GetHandler(String key, Type service)
		{
			if (key == null) throw new ArgumentNullException("key");
			if (service == null) throw new ArgumentNullException("service");

			IHandler selectorsOpinion = GetSelectorsOpinion(key, service);
			if (selectorsOpinion != null)
				return selectorsOpinion;

			using (@lock.ForReading())
			{
				IHandler handler;

				key2Handler.TryGetValue(key, out handler);

				return handler;
			}
		}

		public virtual IHandler[] GetHandlers(Type service)
		{
			if (service == null) throw new ArgumentNullException("service");

			IHandler[] result;
			using (@lock.ForReading())
			{
				if (handlerListsByTypeCache.TryGetValue(service, out result))
					return result;
			}

			using (@lock.ForWriting())
			{

				var handlers = GetHandlers();

				var list = new List<IHandler>(handlers.Length);
				foreach (IHandler handler in handlers)
				{
					if (service == handler.Service)
					{
						list.Add(handler);
					}
				}

				result = list.ToArray();

				handlerListsByTypeCache[service] = result;

			}

			return result;
		}

		public virtual IHandler[] GetAssignableHandlers(Type service)
		{
			if (service == null) throw new ArgumentNullException("service");

			IHandler[] result;
			using (@lock.ForReading())
			{
				if (assignableHandlerListsByTypeCache.TryGetValue(service, out result))
					return result;
			}

			using (@lock.ForWriting())
			{

				var handlers = GetHandlers();
				var list = new List<IHandler>(handlers.Length);
				foreach (IHandler handler in handlers)
				{
					Type handlerService = handler.Service;
					if (service.IsAssignableFrom(handlerService))
					{
						list.Add(handler);
					}
					else
					{
						if (service.IsGenericType &&
						    service.GetGenericTypeDefinition().IsAssignableFrom(handlerService))
						{
							list.Add(handler);
						}
					}
				}

				result = list.ToArray();
				assignableHandlerListsByTypeCache[service] = result;
			}

			return result;
		}

		public virtual IHandler[] GetHandlers()
		{
			using (@lock.ForReading())
			{

				if (allHandlersCache != null)
					return allHandlersCache;

				var list = new IHandler[key2Handler.Values.Count];

				key2Handler.Values.CopyTo(list, 0);

				allHandlersCache = list;

				return list;
			}
		}

		public virtual IHandler this[Type service]
		{
			set
			{
				using (@lock.ForWriting())
				{
					service2Handler[service] = value;
				}
			}
		}

		public virtual IHandler this[String key]
		{
			set
			{
				using (@lock.ForWriting())
				{
					key2Handler[key] = value;
					allHandlers.Add(value);
				}
			}
		}

		public IDictionary<string,IHandler> GetKey2Handler()
		{
			return key2Handler;
		}

		public IDictionary<Type, IHandler> GetService2Handler()
		{
			return service2Handler;
		}

		public void AddHandlerSelector(IHandlerSelector selector)
		{
			selectors.Add(selector);
		}

		protected virtual IHandler GetSelectorsOpinion(string key, Type type)
		{
			type = type ?? typeof(object);// if type is null, we want everything, so object does well for that
			IHandler[] handlers = null;//only init if we have a selector with an opinion about this type
			foreach (IHandlerSelector selector in selectors)
			{
				if (selector.HasOpinionAbout(key, type) == false)
					continue;
				if (handlers == null)
					handlers = GetAssignableHandlers(type);
				IHandler handler = selector.SelectHandler(key, type, handlers);
				if (handler != null)
					return handler;
			}
			return null;
		}

		#endregion
	}
}
