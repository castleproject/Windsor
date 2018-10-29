// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>Default implementation for <see cref = "IDependencyResolver" />. This implementation is quite simple, but still should be useful for 99% of situations.</summary>
	[Serializable]
	public class DefaultDependencyResolver : IDependencyResolver
	{
		private readonly IList<ISubDependencyResolver> subResolvers = new List<ISubDependencyResolver>();
		private ITypeConverter converter;
		private DependencyDelegate dependencyResolvingDelegate;
		private IKernelInternal kernel;

		/// <summary>Registers a sub resolver instance</summary>
		/// <param name = "subResolver">The subresolver instance</param>
		public void AddSubResolver(ISubDependencyResolver subResolver)
		{
			if (subResolver == null)
			{
				throw new ArgumentNullException("subResolver");
			}

			subResolvers.Add(subResolver);
		}

		/// <summary>Initializes this instance with the specified dependency delegate.</summary>
		/// <param name = "kernel">kernel</param>
		/// <param name = "dependencyDelegate">The dependency delegate.</param>
		public void Initialize(IKernelInternal kernel, DependencyDelegate dependencyDelegate)
		{
			this.kernel = kernel;
			converter = kernel.GetConversionManager();
			dependencyResolvingDelegate = dependencyDelegate;
		}

		/// <summary>Unregisters a sub resolver instance previously registered</summary>
		/// <param name = "subResolver">The subresolver instance</param>
		public void RemoveSubResolver(ISubDependencyResolver subResolver)
		{
			subResolvers.Remove(subResolver);
		}

		/// <summary>Returns true if the resolver is able to satisfy the specified dependency.</summary>
		/// <param name = "context">Creation context, which is a resolver itself</param>
		/// <param name = "contextHandlerResolver">Parent resolver</param>
		/// <param name = "model">Model of the component that is requesting the dependency</param>
		/// <param name = "dependency">The dependency model</param>
		/// <returns>
		///     <c>true</c>
		///     if the dependency can be satisfied
		/// </returns>
		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			// 1 - check for the dependency on CreationContext, if present
			if (CanResolveFromContext(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 2 - check with the model's handler, if not the same as the parent resolver
			if (CanResolveFromHandler(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 3 - check within parent resolver, if present
			if (CanResolveFromContextHandlerResolver(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 4 - check within subresolvers
			if (CanResolveFromSubResolvers(context, contextHandlerResolver, model, dependency))
			{
				return true;
			}

			// 5 - normal flow, checking against the kernel
			return CanResolveFromKernel(context, model, dependency);
		}

		/// <summary>Try to resolve the dependency by checking the parameters in the model or checking the Kernel for the requested service.</summary>
		/// <remarks>
		///     The dependency resolver has the following precedence order:
		///     <list type = "bullet">
		///         <item>
		///             <description>The dependency is checked within the
		///                 <see cref = "CreationContext" />
		///             </description>
		///         </item>
		///         <item>
		///             <description>The dependency is checked within the
		///                 <see cref = "IHandler" />
		///                 instance for the component</description>
		///         </item>
		///         <item>
		///             <description>The dependency is checked within the registered
		///                 <see cref = "ISubDependencyResolver" />
		///                 s</description>
		///         </item>
		///         <item>
		///             <description>Finally the resolver tries the normal flow which is using the configuration or other component to satisfy the dependency</description>
		///         </item>
		///     </list>
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
					var message = String.Format(
						"Could not resolve non-optional dependency for '{0}' ({1}). Parameter '{2}' type '{3}'",
						model.Name,
						model.Implementation != null ? model.Implementation.FullName : "-unknown-",
						dependency.DependencyKey,
						dependency.TargetType.FullName);

					throw new DependencyResolverException(message);
				}
			}

			dependencyResolvingDelegate(model, dependency, value);
			return value;
		}

		protected virtual bool CanResolveFromKernel(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			if (dependency.ReferencedComponentName != null)
			{
				// User wants to override
				return HasComponentInValidState(dependency.ReferencedComponentName, dependency, context);
			}
			if (dependency.Parameter != null)
			{
				return true;
			}
			if (typeof(IKernel).IsAssignableFrom(dependency.TargetItemType))
			{
				return true;
			}

			if (dependency.TargetItemType.IsPrimitiveType())
			{
				return false;
			}

			return HasAnyComponentInValidState(dependency.TargetItemType, dependency, context);
		}

		/// <summary>This method rebuild the context for the parameter type. Naive implementation.</summary>
		protected virtual CreationContext RebuildContextForParameter(CreationContext current, Type parameterType)
		{
			if (parameterType.GetTypeInfo().ContainsGenericParameters)
			{
				return current;
			}

			return new CreationContext(parameterType, current, false);
		}

		protected virtual object ResolveFromKernel(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			if (dependency.ReferencedComponentName != null)
			{
				return ResolveFromKernelByName(context, model, dependency);
			}
			if (dependency.Parameter != null)
			{
				return ResolveFromParameter(context, model, dependency);
			}
			if (typeof(IKernel).IsAssignableFrom(dependency.TargetItemType))
			{
				return kernel;
			}
			if (dependency.TargetItemType.IsPrimitiveType())
			{
				// we can shortcircuit it here, since we know we won't find any components for value type service as those are not legal.
				return null;
			}

			return ResolveFromKernelByType(context, model, dependency);
		}

		private bool CanResolveFromContext(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
			DependencyModel dependency)
		{
			return context != null && context.CanResolve(context, contextHandlerResolver, model, dependency);
		}

		private bool CanResolveFromContextHandlerResolver(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model, DependencyModel dependency)
		{
			return contextHandlerResolver != null && contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency);
		}

		private bool CanResolveFromHandler(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
			DependencyModel dependency)
		{
			var handler = kernel.GetHandler(model.Name);
			var b = handler != null && handler != contextHandlerResolver && handler.CanResolve(context, contextHandlerResolver, model, dependency);
			return b;
		}

		private bool CanResolveFromSubResolvers(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
			DependencyModel dependency)
		{
			return subResolvers.Count > 0 && subResolvers.Any(s => s.CanResolve(context, contextHandlerResolver, model, dependency));
		}

		private bool HasAnyComponentInValidState(Type service, DependencyModel dependency, CreationContext context)
		{
			IHandler firstHandler;
			if (context != null && context.IsResolving)
			{
				firstHandler = kernel.LoadHandlerByType(dependency.DependencyKey, service, context.AdditionalArguments);
			}
			else
			{
				firstHandler = kernel.GetHandler(service);
			}
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
			var nonResolvingHandlers = handlers.Where(handler => handler.IsBeingResolvedInContext(context) == false).ToList();
			RebuildOpenGenericHandlersWithClosedGenericSubHandlers(service, context, nonResolvingHandlers);
			return nonResolvingHandlers.Any(handler => IsHandlerInValidState(handler));
		}

		private void RebuildOpenGenericHandlersWithClosedGenericSubHandlers(Type service, CreationContext context, List<IHandler> nonResolvingHandlers)
		{
			if (context.RequestedType != null && service.GetTypeInfo().IsGenericType)
			{
				// Remove DefaultGenericHandlers
				var genericHandlers = nonResolvingHandlers.OfType<DefaultGenericHandler>().ToList();
				nonResolvingHandlers.RemoveAll(x => genericHandlers.Contains(x));

				// Convert open generic handlers to closed generic sub handlers 
				var openGenericContext = RebuildContextForParameter(context, service);
				var closedGenericSubHandlers = genericHandlers.Select(x => x.ConvertToClosedGenericHandler(service, openGenericContext)).ToList();

				// Update nonResolvingHandlers with closed generic sub handlers with potentially valid state
				nonResolvingHandlers.AddRange(closedGenericSubHandlers);
			}
		}

		private bool HasComponentInValidState(string key, DependencyModel dependency, CreationContext context)
		{
			IHandler handler;
			if (context != null && context.IsResolving)
			{
				handler = kernel.LoadHandlerByName(key, dependency.TargetItemType, context.AdditionalArguments);
			}
			else
			{
				handler = kernel.GetHandler(key);
			}
			return IsHandlerInValidState(handler) && handler.IsBeingResolvedInContext(context) == false;
		}

		private object ResolveCore(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			// 1 - check for the dependency on CreationContext, if present
			if (CanResolveFromContext(context, contextHandlerResolver, model, dependency))
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
			if (CanResolveFromContextHandlerResolver(context, contextHandlerResolver, model, dependency))
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
			return ResolveFromKernel(context, model, dependency);
		}

		private object ResolveFromKernelByName(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			var handler = kernel.LoadHandlerByName(dependency.ReferencedComponentName, dependency.TargetItemType, context.AdditionalArguments);

			// never (famous last words) this should really happen as we're the good guys and we call CanResolve before trying to resolve but let's be safe.
			if (handler == null)
			{
				throw new DependencyResolverException(
					string.Format(
						"Missing dependency.{2}Component {0} has a dependency on component {1}, which was not registered.{2}Make sure the dependency is correctly registered in the container as a service.",
						model.Name,
						dependency.ReferencedComponentName,
						Environment.NewLine));
			}

			var contextRebuilt = RebuildContextForParameter(context, dependency.TargetItemType);

			return handler.Resolve(contextRebuilt);
		}

		private object ResolveFromKernelByType(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			if (!TryGetHandlerFromKernel(dependency, context, out var handler))
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
						Environment.NewLine));
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
			context = RebuildContextForParameter(context, dependency.TargetItemType);

			return handler.Resolve(context);
		}

		private object ResolveFromParameter(CreationContext context, ComponentModel model, DependencyModel dependency)
		{
			converter.Context.Push(model, context);
			try
			{
				if (dependency.Parameter.Value != null || dependency.Parameter.ConfigValue == null)
				{
					return converter.PerformConversion(dependency.Parameter.Value, dependency.TargetItemType);
				}
				else
				{
					return converter.PerformConversion(dependency.Parameter.ConfigValue, dependency.TargetItemType);
				}
			}
			catch (ConverterException e)
			{
				throw new DependencyResolverException(
					string.Format("Could not convert parameter '{0}' to type '{1}'.", dependency.Parameter.Name, dependency.TargetItemType.Name), e);
			}
			finally
			{
				converter.Context.Pop();
			}
		}

		private bool TryGetHandlerFromKernel(DependencyModel dependency, CreationContext context, out IHandler handler)
		{
			// we are doing it in two stages because it is likely to be faster to a lookup
			// by key than a linear search
			try
			{
				handler = kernel.LoadHandlerByType(dependency.DependencyKey, dependency.TargetItemType, context.AdditionalArguments);
			}
			catch (HandlerException)
			{
				handler = null;
			}
			if (handler == null) return false;

			if (handler.IsBeingResolvedInContext(context) == false)
			{
				return true;
			}

			// make a best effort to find another one that fit

			IHandler[] handlers;
			try
			{
				handlers = kernel.GetHandlers(dependency.TargetItemType);
			}
			catch (HandlerException)
			{
				return false;
			}

			foreach (var maybeCorrectHandler in handlers)
			{
				if (maybeCorrectHandler.IsBeingResolvedInContext(context) == false)
				{
					handler = maybeCorrectHandler;
					break;
				}
			}
			return true;
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