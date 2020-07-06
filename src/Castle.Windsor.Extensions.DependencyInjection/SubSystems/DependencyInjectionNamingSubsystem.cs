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


namespace Castle.Windsor.Extensions.DependencyInjection.SubSystems
{
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.MicroKernel.Util;
	
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Naming subsystem based on DefaultNamingSubSystem but GetHandlers returns handlers in registration order
	/// </summary>
	internal class DependencyInjectionNamingSubsystem :  DefaultNamingSubSystem
	{
		private readonly IDictionary<Type, IHandler[]> handlerListsRegistrationOrderByTypeCache =
			new Dictionary<Type, IHandler[]>(SimpleTypeEqualityComparer.Instance);

		private IHandler[] GetHandlersInRegisterOrderNoLock(Type service)
		{
			return name2Handler.Values.Where(handler => handler.Supports(service)).ToArray();
		}

		public override IHandler[] GetHandlers(Type service)
		{
			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
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
				if (handlerListsRegistrationOrderByTypeCache.TryGetValue(service, out result))
				{
					return result;
				}
				result = GetHandlersInRegisterOrderNoLock(service);

				locker.Upgrade();
				handlerListsRegistrationOrderByTypeCache[service] = result;
			}

			return result;
		}

		public override IHandler GetHandler(Type service)
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

			if (HandlerByServiceCache.TryGetValue(service, out var handler))
			{
				return handler;
			}

			if (!service.GetTypeInfo().IsGenericType || service.GetTypeInfo().IsGenericTypeDefinition) return null;
			var openService = service.GetGenericTypeDefinition();
			if (HandlerByServiceCache.TryGetValue(openService, out handler) && handler.Supports(service))
			{
				return handler;
			}

			//use original, priority-based GetHandlers
			var handlerCandidates = base.GetHandlers(openService);
			return handlerCandidates.FirstOrDefault(handlerCandidate => handlerCandidate.Supports(service));

		}

	}
}