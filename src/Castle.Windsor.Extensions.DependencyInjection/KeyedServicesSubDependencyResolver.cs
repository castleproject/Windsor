#if NET8_0_OR_GREATER
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Castle.Windsor.Extensions.DependencyInjection
{
	internal class KeyedServicesSubDependencyResolver : ISubDependencyResolver
	{
		private readonly IWindsorContainer _container;

		public KeyedServicesSubDependencyResolver(IWindsorContainer container)
		{
			_container = container;
		}

		/// <summary>
		/// We cache in key all the factories that we have for specific constructor dependencies.
		/// </summary>
		private readonly Dictionary<string, Func<Object>> _factoriesCache = new();

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			if (dependency is ConstructorDependencyModel cdm)
			{
				var cacheKey = GetFactoryCacheKey(cdm);
				Func<Object> factory;
				if (!_factoriesCache.TryGetValue(cacheKey, out factory))
				{
					//we are resolving a dependency of a constructor,where the parameter could be resolved by a keyed service
					var constructorParameter = cdm.Constructor.Constructor.GetParameters().Single(p => p.Name == cdm.DependencyKey);
					var attribute = constructorParameter.GetCustomAttribute<FromKeyedServicesAttribute>();
					if (attribute != null)
					{
						var krh = KeyedRegistrationHelper.GetInstance(_container);
						var registration = krh.GetKey(attribute.Key, cdm.TargetItemType);
						//ok this parameter has an attribute, so we can cache
						factory = () => registration.Resolve(_container);
					}
					_factoriesCache[cacheKey] = factory;
				}

				if (factory != null)
				{
					return true;
				}
			}
			return false;
		}

		private static string GetFactoryCacheKey(ConstructorDependencyModel cdm)
		{
			return $"{cdm.TargetType.FullName}+{cdm.DependencyKey}";
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			var cdm = (ConstructorDependencyModel)dependency;

			var cacheKey = GetFactoryCacheKey(cdm);
			var factory = _factoriesCache[cacheKey];
			return factory();
		}
	}
}
#endif