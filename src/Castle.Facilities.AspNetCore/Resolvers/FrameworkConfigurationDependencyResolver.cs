// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore.Resolvers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	using Microsoft.Extensions.DependencyInjection;

	public class FrameworkConfigurationDependencyResolver : ISubDependencyResolver
	{
		private readonly ServiceProvider serviceProvider;
		private readonly IServiceCollection serviceCollection;

		public FrameworkConfigurationDependencyResolver(IServiceCollection serviceCollection)
		{
			this.serviceCollection = serviceCollection;
			serviceProvider = serviceCollection.BuildServiceProvider();
		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			var dependencyType = dependency.TargetType;

			if (dependencyType.IsGenericType && HasMatchingGenericTypeDefinitions(dependencyType))
			{
				return true;
			}

			return serviceCollection.Any(x => x.ServiceType == dependencyType);
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			return serviceProvider.GetService(dependency.TargetType);
		}

		private bool HasMatchingGenericTypeDefinitions(Type dependencyType)
		{
			var dependencyGenericType = dependencyType.GetGenericTypeDefinition();
			var genericServiceTypes = serviceCollection.Where(x => x.ServiceType.IsGenericType).ToList();

			if (HasMatchingGenericTypesWithArguments(dependencyType, dependencyGenericType, genericServiceTypes))
			{
				return true;
			}

			return HasMatchingGenericTypesWithoutArguments(dependencyGenericType, genericServiceTypes);
		}

		private static bool HasMatchingGenericTypesWithArguments(Type dependencyType, Type dependencyGenericType, IEnumerable<ServiceDescriptor> genericServiceTypes)
		{
			var genericTypesWithParameters = genericServiceTypes.Where(
				x => x.ServiceType.GenericTypeArguments.Length > 0).ToList();

			return genericTypesWithParameters.Any(
				x => x.ServiceType.GetGenericTypeDefinition() == dependencyGenericType 
					&& x.ServiceType.GenericTypeArguments.All(
						sy => dependencyType.GenericTypeArguments.Contains(sy) 
							&& dependencyType.GenericTypeArguments.Length == 
								x.ServiceType.GenericTypeArguments.Length));
		}

		private static bool HasMatchingGenericTypesWithoutArguments(Type dependencyGenericType, List<ServiceDescriptor> genericServiceTypes)
		{
			var genericTypesWithoutParameters = genericServiceTypes.Where(
				x => x.ServiceType.GenericTypeArguments.Length == 0 
					&& x.ImplementationType.GenericTypeArguments.Length == 0).ToList();

			return genericTypesWithoutParameters.Any(
				x => x.ServiceType == dependencyGenericType);
		}
	}
}