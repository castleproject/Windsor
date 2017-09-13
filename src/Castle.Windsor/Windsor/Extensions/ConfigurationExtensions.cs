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

namespace Castle.Windsor.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.SubSystems.Naming;

	public static class ConfigurationExtensions
	{
		public static bool ValidateConfiguration(this IWindsorContainer container)
		{
			var message = string.Empty;
			return container.ValidateConfiguration(out message);
		}

		public static bool ValidateConfiguration(this IWindsorContainer container, out string message)
		{
			var unresolvables = new List<(DependencyModel dependency, IHandler handler)>();
			var visitedHandlers = new HashSet<IHandler>();

			var naming = (INamingSubSystem) container.Kernel.GetSubSystem(SubSystemConstants.NamingKey);
			foreach (var handler in naming.GetAllHandlers())
				GetUnresolvableDependencies(handler, visitedHandlers, container, unresolvables);

			if (unresolvables.Count == 0)
			{
				message = null;
				return true;
			}

			var stringBuilder = new StringBuilder("Unresolvable dependencies:");
			foreach (var dependentsByDependency in unresolvables.GroupBy(_ => (type: _.dependency.TargetItemType, name: _.dependency.ReferencedComponentName)))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append(dependentsByDependency.Key.type);
				if (dependentsByDependency.Key.name != null)
					stringBuilder.Append(" named \"").Append(dependentsByDependency.Key.name);
				stringBuilder.Append(":");

				GetDependendants(dependentsByDependency, stringBuilder);
			}

			message = stringBuilder.ToString();
			return false;
		}

		private static void GetDependendants(IGrouping<ValueTuple<Type, string>, ValueTuple<DependencyModel, IHandler>> dependentsByDependency, StringBuilder stringBuilder)
		{
			foreach (var linksByDependentNames in dependentsByDependency.GroupBy<(DependencyModel dependency, IHandler handler), string>(_ => _.handler.ComponentModel.Name))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(" - ");

				var isFirst = true;
				foreach (var link in linksByDependentNames)
				{
					if (isFirst) isFirst = false;
					else stringBuilder.Append(", ");
					stringBuilder.Append($"{link.dependency.DependencyKey}");
				}

				stringBuilder.Append(" for ").Append(linksByDependentNames.Key);
			}
		}

		private static void GetUnresolvableDependencies(IHandler handler, HashSet<IHandler> visitedHandlers, IWindsorContainer container, List<(DependencyModel dependency, IHandler handler)> unresolvables)
		{
			if (handler.CurrentState == HandlerState.Valid || !visitedHandlers.Add(handler)) return;
			foreach (var dependency in handler.ComponentModel.Dependencies)
			{
				if (dependency.IsOptional || container.Kernel.Resolver.CanResolve(CreationContext.CreateEmpty(), handler, handler.ComponentModel, dependency))
					continue;

				var dependencyHandler = container.Kernel.GetHandler(dependency.TargetItemType);
				if (dependencyHandler != null)
					GetUnresolvableDependencies(dependencyHandler, visitedHandlers, container, unresolvables);
				else
					unresolvables.Add((dependency, handler));
			}
		}
	}
}