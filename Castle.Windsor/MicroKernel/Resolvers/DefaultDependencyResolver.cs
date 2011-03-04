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

namespace Castle.MicroKernel.Resolvers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.Util;

	/// <summary>
	///   Default implementation for <see cref = "IDependencyResolver" />.
	///   This implementation is quite simple, but still should be useful
	///   for 99% of situations.
	/// </summary>
	[Serializable]
	public class DefaultDependencyResolver : IDependencyResolver
	{
		private readonly IList<ISubDependencyResolver> subResolvers = new List<ISubDependencyResolver>();
		private ITypeConverter converter;
		private DependencyDelegate dependencyResolvingDelegate;
		private IKernelInternal kernel;

		/// <summary>
		///   Registers a sub resolver instance
		/// </summary>
		/// <param name = "subResolver">The subresolver instance</param>
		public void AddSubResolver(ISubDependencyResolver subResolver)
		{
			if (subResolver == null)
			{
				throw new ArgumentNullException("subResolver");
			}

			subResolvers.Add(subResolver);
		}

		/// <summary>
		///   Initializes this instance with the specified dependency delegate.
		/// </summary>
		/// <param name = "kernel">kernel</param>
		/// <param name = "dependencyDelegate">The dependency delegate.</param>
		public void Initialize(IKernelInternal kernel, DependencyDelegate dependencyDelegate)
		{
			this.kernel = kernel;
			converter = kernel.GetConversionManager();
			dependencyResolvingDelegate = dependencyDelegate;
		}

		/// <summary>
		///   Unregisters a sub resolver instance previously registered
		/// </summary>
		/// <param name = "subResolver">The subresolver instance</param>
		public void RemoveSubResolver(ISubDependencyResolver subResolver)
		{
			subResolvers.Remove(subResolver);
		}

		/// <summary>
		///   Returns true if the resolver is able to satisfy the specified dependency.
		/// </summary>
		/// <param name = "context">Creation context, which is a resolver itself</param>
		/// <param name = "contextHandlerResolver">Parent resolver</param>
		/// <param name = "model">Model of the component that is requesting the dependency</param>
		/// <param name = "dependency">The dependency model</param>
		/// <returns>
		///   <c>true</c>
		///   if the dependency can be satisfied</returns>
		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			// 1 - check for the dependency on CreationContext, if present

			if (context != null && context.CanResolve(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 2 - check with the model's handler, if not the same as the parent resolver

			var handler = kernel.GetHandler(model.Name);
			if (handler != null && handler != contextHandlerResolver && handler.CanResolve(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 3 - check within parent resolver, if present

			if (contextHandlerResolver != null && contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 4 - check within subresolvers

			if (subResolvers.Count > 0)
			{
				if (subResolvers.Any(s => s.CanResolve(context, contextHandlerResolver, model, dependency)))
				{
					return true;
				}
			}

			// 5 - normal flow, checking against the kernel
			return CanResolveCore(context, model, dependency);
		}

		/// <summary>
		///   Try to resolve the dependency by checking the parameters in 
		///   the model or checking the Kernel for the requested service.
		/// </summary>
		/// <remarks>
		///   The dependency resolver has the following precedence order:
		///   <list type = "bullet">
		///     <item>
		///       <description>The dependency is checked within the
		///         <see cref = "CreationContext" />
		///       </description>
		///     </item>
		///     <item>
		///       <description>The dependency is checked within the
		///         <see cref = "IHandler" />
		///         instance for the component</description>
		///     </item>
		///     <item>
		///       <description>The dependency is checked within the registered
		///         <see cref = "ISubDependencyResolver" />
		///         s</description>
		///     </item>
		///     <item>
		///       <description>Finally the resolver tries the normal flow 
		///         which is using the configuration
		///         or other component to satisfy the dependency</description>
		///     </item>
		///   </list>
		/// </remarks>
		/// <param name = "context">Creation context, which is a resolver itself</param>
		/// <param name = "contextHandlerResolver">Parent resolver</param>
		/// <param name = "model">Model of the component that is requesting the dependency</param>
		/// <param name = "dependency">The dependency model</param>
		/// <returns>The dependency resolved value or null</returns>
		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			var value = ResolveCore(context, contextHandlerResolver, model, dependency);
			if (value == null)
			{
				if (dependency.HasDefaultValue)
				{
					value = dependency.DefaultValue;
				}
				else if (dependency.IsOptional == false)
				{
					var implementation = String.Empty;
					if (model.Implementation != null)
					{
						implementation = model.Implementation.FullName;
					}

					var message = String.Format(
						"Could not resolve non-optional dependency for '{0}' ({1}). Parameter '{2}' type '{3}'",
						model.Name, implementation, dependency.DependencyKey, dependency.TargetType.FullName);

					throw new DependencyResolverException(message);
				}
			}

			RaiseDependencyResolving(model, dependency, value);

			return value;
		}

		protected virtual bool CanResolveCore(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			return CanResolveServiceDependencyMandatory(dependency, model, context);
		}

		protected virtual ParameterModel ObtainParameterModelMatchingDependency(DependencyModel dependency, ComponentModel model)
		{
			if(model.HasParameters == false)
			{
				return null;
			}
			return model.Parameters.GetMatch(dependency);
		}

		/// <summary>
		///   This method rebuild the context for the parameter type.
		///   Naive implementation.
		/// </summary>
		protected virtual CreationContext RebuildContextForParameter(CreationContext current, Type parameterType)
		{
			if (parameterType.ContainsGenericParameters)
			{
				return current;
			}

			return new CreationContext(parameterType, current, false);
		}

		protected virtual object ResolveCore(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			var serviceOverrideComponent = default(string);
			var parameter = ObtainParameterModelMatchingDependency(dependency, model);
			if (parameter != null)
			{
				if ((serviceOverrideComponent = ReferenceExpressionUtil.ExtractComponentKey(parameter.Value)) == null)
				{
					converter.Context.Push(model, context);

					try
					{
						if (parameter.Value != null || parameter.ConfigValue == null)
						{
							return converter.PerformConversion(parameter.Value, dependency.TargetItemType);
						}
						else
						{
							return converter.PerformConversion(parameter.ConfigValue, dependency.TargetItemType);
						}
					}
					finally
					{
						converter.Context.Pop();
					}
				}
			}

			if (typeof(IKernel).IsAssignableFrom(dependency.TargetItemType))
			{
				return kernel;
			}
			IHandler handler;
			if (serviceOverrideComponent != null)
			{
				handler = kernel.GetHandler(serviceOverrideComponent);
			}
			else
			{
				try
				{
					handler = TryGetHandlerFromKernel(dependency, context);
				}
				catch (HandlerException exception)
				{
					if (dependency.HasDefaultValue)
					{
						return dependency.DefaultValue;
					}
					throw new DependencyResolverException(
						string.Format(
							"Missing dependency.{2}Component {0} has a dependency on {1}, which could not be resolved.{2}Make sure the dependency is correctly registered in the container as a service, or provided as inline argument.",
							model.Name,
							dependency.TargetItemType,
							Environment.NewLine),
						exception);
				}

				if (handler == null)
				{
					if (dependency.HasDefaultValue)
					{
						return dependency.DefaultValue;
					}
					throw new DependencyResolverException(
						string.Format(
							"Cycle detected in configuration.{2}Component {0} has a dependency on {1}, but it doesn't provide an override.{2}You must provide an override if a component has a dependency on a service that it - itself - provides.",
							model.Name,
							dependency.TargetItemType,
							Environment.NewLine));
				}
			}
			if (handler == null)
			{
				return null;
			}

			context = RebuildContextForParameter(context, dependency.TargetItemType);

			return handler.Resolve(context);
		}

		private bool CanResolveServiceDependencyMandatory(DependencyModel dependency, ComponentModel model, CreationContext context)
		{
			var parameter = ObtainParameterModelMatchingDependency(dependency, model);
			if (parameter != null)
			{
				if (ReferenceExpressionUtil.IsReference(parameter.Value))
				{
					// User wants to override
					var value = ReferenceExpressionUtil.ExtractComponentKey(parameter.Value);
					return HasComponentInValidState(value, dependency.TargetItemType, context);
				}
				return ObtainParameterModelMatchingDependency(dependency, model) != null;
			}
			if (typeof(IKernel).IsAssignableFrom(dependency.TargetItemType))
			{
				return true;
			}

			// Default behaviour
			if (dependency.TargetItemType != null)
			{
				return HasAnyComponentInValidState(context, dependency.TargetItemType, dependency.DependencyKey, GetAdditionalArguments(context));
			}
			return HasComponentInValidState(dependency.DependencyKey, dependency.TargetItemType, context);
		}

		private bool HasAnyComponentInValidState(CreationContext context, Type service, string name, IDictionary arguments)
		{
			var firstHandler = kernel.LoadHandlerByType(name, service, arguments);
			if (firstHandler == null)
			{
				return false;
			}
			if (context == null || firstHandler.IsBeingResolvedInContext(context) == false)
			{
				if (IsHandlerInValidState(firstHandler))
				{
					return true;
				}
			}

			var handlers = kernel.GetHandlers(service);
			foreach (var handler in handlers)
			{
				if (context == null || handler.IsBeingResolvedInContext(context) == false)
				{
					return IsHandlerInValidState(handler);
				}
			}

			return false;
		}

		private bool HasComponentInValidState(string key, Type type, CreationContext context)
		{
			var handler = kernel.LoadHandlerByKey(key, type, GetAdditionalArguments(context));
			return IsHandlerInValidState(handler) && handler.IsBeingResolvedInContext(context) == false;
		}


		private void RaiseDependencyResolving(ComponentModel model, DependencyModel dependency, object value)
		{
			dependencyResolvingDelegate(model, dependency, value);
		}

		private object ResolveCore(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			// 1 - check for the dependency on CreationContext, if present
			if (context != null && context.CanResolve(context, contextHandlerResolver, model, dependency))
			{
				return context.Resolve(context, contextHandlerResolver, model, dependency);
			}

			// 2 - check with the model's handler, if not the same as the parent resolver
			var handler = kernel.GetHandler(model.Name);
			if (handler != contextHandlerResolver && handler.CanResolve(context, contextHandlerResolver, model, dependency))
			{
				return handler.Resolve(context, contextHandlerResolver, model, dependency);
			}

			// 3 - check within parent resolver, if present
			if (contextHandlerResolver != null && contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency))
			{
				return contextHandlerResolver.Resolve(context, contextHandlerResolver, model, dependency);
			}

			// 4 - check within subresolvers
			if (subResolvers.Count > 0)
			{
				for (var index = 0; index < subResolvers.Count; index++)
				{
					var subResolver = subResolvers[index];
					if (subResolver.CanResolve(context, contextHandlerResolver, model, dependency))
					{
						return subResolver.Resolve(context, contextHandlerResolver, model, dependency);
					}
				}
			}

			// 5 - normal flow, checking against the kernel
			return ResolveCore(context, model, dependency);
		}

		private IHandler TryGetHandlerFromKernel(DependencyModel dependency, CreationContext context)
		{
			// we are doing it in two stages because it is likely to be faster to a lookup
			// by key than a linear search
			var itemType = dependency.TargetItemType;
			var handler = kernel.GetHandler(itemType);
			if (handler == null)
			{
				throw new HandlerException(string.Format("Handler for {0} was not found.", itemType));
			}
			if (handler.IsBeingResolvedInContext(context) == false)
			{
				return handler;
			}

			// make a best effort to find another one that fit

			var handlers = kernel.GetHandlers(itemType);
			foreach (var maybeCorrectHandler in handlers)
			{
				if (maybeCorrectHandler.IsBeingResolvedInContext(context) == false)
				{
					handler = maybeCorrectHandler;
					break;
				}
			}
			return handler;
		}

		private static IDictionary GetAdditionalArguments(CreationContext context)
		{
			if (context != null)
			{
				return context.AdditionalArguments;
			}
			return null;
		}

		private static bool IsHandlerInValidState(IHandler handler)
		{
			if (handler == null)
			{
				return false;
			}

			return handler.CurrentState == HandlerState.Valid;
		}
	}
}