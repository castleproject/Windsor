// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.Util;

	/// <summary>
	/// Default implementation for <see cref="IDependencyResolver"/>.
	/// This implementation is quite simple, but still should be useful
	/// for 99% of situations. 
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class DefaultDependencyResolver : IDependencyResolver
	{
		private readonly IKernel kernel;
		private readonly ITypeConverter converter;
		private readonly IList<ISubDependencyResolver> subResolvers = new List<ISubDependencyResolver>();
		private DependencyDelegate dependencyResolvingDelegate;

		/// <summary>
		///   Initializes a new instance of the
		///   <see cref = "DefaultDependencyResolver" />
		///   class.
		/// </summary>
		/// <param name = "kernel">The kernel.</param>
		public DefaultDependencyResolver(IKernel kernel)
		{
			this.kernel = kernel;

			converter = (ITypeConverter)kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
		}

		/// <summary>
		///   Initializes this instance with the specified dependency delegate.
		/// </summary>
		/// <param name = "dependencyDelegate">The dependency delegate.</param>
		public void Initialize(DependencyDelegate dependencyDelegate)
		{
			dependencyResolvingDelegate = dependencyDelegate;
		}

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
		///   Unregisters a sub resolver instance previously registered
		/// </summary>
		/// <param name = "subResolver">The subresolver instance</param>
		public void RemoveSubResolver(ISubDependencyResolver subResolver)
		{
			if (subResolver == null)
			{
				throw new ArgumentNullException("subResolver");
			}

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
		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                       DependencyModel dependency)
		{
			// 1 - check for the dependency on CreationContext, if present

			if (context != null)
			{
				if (context.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					return true;
				}
			}

			// 2 - check with the model's handler, if not the same as the parent resolver

			var handler = kernel.GetHandler(model.Name);
			if (handler != null && handler != contextHandlerResolver)
			{
				if (handler.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					return true;
				}
			}

			// 3 - check within parent resolver, if present

			if (contextHandlerResolver != null)
			{
				if (contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					return true;
				}
			}

			// 4 - check within subresolvers

			foreach (ISubDependencyResolver subResolver in subResolvers)
			{
				if (subResolver.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					return true;
				}
			}

			// 5 - normal flow, checking against the kernel

			if (dependency.DependencyType == DependencyType.Service ||
			    dependency.DependencyType == DependencyType.ServiceOverride)
			{
				return CanResolveServiceDependency(context, model, dependency);
			}
			return CanResolveParameterDependency(model, dependency);
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
		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                      DependencyModel dependency)
		{
			object value = null;

			bool resolved = false;

			// 1 - check for the dependency on CreationContext, if present

			if (context != null)
			{
				if (context.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					value = context.Resolve(context, contextHandlerResolver, model, dependency);
					resolved = true;
				}
			}

			// 2 - check with the model's handler, if not the same as the parent resolver

			IHandler handler = kernel.GetHandler(model.Name);

			if (!resolved && handler != contextHandlerResolver)
			{
				if (handler.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					value = handler.Resolve(context, contextHandlerResolver, model, dependency);
					resolved = true;
				}
			}

			// 3 - check within parent resolver, if present

			if (!resolved && contextHandlerResolver != null)
			{
				if (contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency))
				{
					value = contextHandlerResolver.Resolve(context, contextHandlerResolver, model, dependency);
					resolved = true;
				}
			}

			// 4 - check within subresolvers

			if (!resolved)
			{
				foreach (ISubDependencyResolver subResolver in subResolvers)
				{
					if (subResolver.CanResolve(context, contextHandlerResolver, model, dependency))
					{
						value = subResolver.Resolve(context, contextHandlerResolver, model, dependency);
						resolved = true;
						break;
					}
				}
			}

			// 5 - normal flow, checking against the kernel

			if (!resolved)
			{
				if (dependency.DependencyType == DependencyType.Service ||
				    dependency.DependencyType == DependencyType.ServiceOverride)
				{
					value = ResolveServiceDependency(context, model, dependency);
				}
				else
				{
					value = ResolveParameterDependency(context, model, dependency);
				}
			}

			if (value == null)
			{
				if (dependency.HasDefaultValue)
				{
					value = dependency.DefaultValue;
				}
				else if (dependency.IsOptional == false)
				{
					String implementation = String.Empty;

					if (model.Implementation != null)
					{
						implementation = model.Implementation.FullName;
					}

					String message = String.Format(
						"Could not resolve non-optional dependency for '{0}' ({1}). Parameter '{2}' type '{3}'",
						model.Name, implementation, dependency.DependencyKey, dependency.TargetType.FullName);

					throw new DependencyResolverException(message);
				}
			}

			RaiseDependencyResolving(model, dependency, value);

			return value;
		}

		protected virtual bool CanResolveServiceDependency(CreationContext context, ComponentModel model,
		                                                   DependencyModel dependency)
		{
			if (dependency.DependencyType == DependencyType.ServiceOverride)
			{
				return HasComponentInValidState(dependency.DependencyKey);
			}

			ParameterModel parameter = ObtainParameterModelMatchingDependency(dependency, model);

			if (parameter != null)
			{
				// User wants to override

				String value = ExtractComponentKey(parameter.Value, parameter.Name);

				return HasComponentInValidState(value);
			}
			if (dependency.TargetItemType == typeof(IKernel))
			{
				return true;
			}
			// Default behaviour

			if (dependency.TargetItemType != null)
			{
				return HasComponentInValidState(context, dependency.TargetItemType);
			}
			return HasComponentInValidState(dependency.DependencyKey);
		}

		protected virtual bool CanResolveParameterDependency(ComponentModel model, DependencyModel dependency)
		{
			ParameterModel parameter = ObtainParameterModelMatchingDependency(dependency, model);

			return parameter != null;
		}

		protected virtual object ResolveServiceDependency(CreationContext context, ComponentModel model,
		                                                  DependencyModel dependency)
		{
			IHandler handler;
			if (dependency.DependencyType == DependencyType.Service)
			{
				ParameterModel parameter = ObtainParameterModelMatchingDependency(dependency, model);

				if (parameter != null)
				{
					// User wants to override, we then 
					// change the type to ServiceOverride
					dependency.DependencyKey = ExtractComponentKey(parameter.Value, parameter.Name);
					dependency.DependencyType = DependencyType.ServiceOverride;
				}
			}

			if (dependency.TargetItemType == typeof(IKernel))
			{
				return kernel;
			}
			if (dependency.DependencyType == DependencyType.ServiceOverride)
			{
				handler = kernel.GetHandler(dependency.DependencyKey);
			}
			else
			{
				try
				{
					handler = TryGetHandlerFromKernel(dependency, context);
				}
				catch (HandlerException exception)
				{
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

			IHandler[] handlers = kernel.GetHandlers(itemType);

			foreach (IHandler maybeCorrectHandler in handlers)
			{
				if (maybeCorrectHandler.IsBeingResolvedInContext(context) == false)
				{
					handler = maybeCorrectHandler;
					break;
				}
			}
			return handler;
		}

		protected virtual object ResolveParameterDependency(CreationContext context, ComponentModel model,
		                                                    DependencyModel dependency)
		{
			ParameterModel parameter = ObtainParameterModelMatchingDependency(dependency, model);

			if (parameter != null)
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

			return null;
		}

		protected virtual ParameterModel ObtainParameterModelMatchingDependency(DependencyModel dependency,
		                                                                        ComponentModel model)
		{
			return ObtainParameterModelByKey(dependency, model) ?? ObtainParameterModelByType(dependency, model);
		}

		private ParameterModel ObtainParameterModelByType(DependencyModel dependency, ComponentModel model)
		{
			var type = dependency.TargetItemType;
			if (type == null)
			{
				// for example it's an interceptor
				return null;
			}
			var parameter = GetParameterModelByType(type, model);
			if (parameter == null && type.IsGenericType)
			{
				parameter = GetParameterModelByType(type.GetGenericTypeDefinition(), model);
			}
			return parameter;
		}

		private ParameterModel GetParameterModelByType(Type type, ComponentModel model)
		{
			if (type == null)
			{
				return null;
			}

			var key = type.AssemblyQualifiedName;
			if (key == null)
			{
				return null;
			}

			return model.Parameters[key];
		}

		private ParameterModel ObtainParameterModelByKey(DependencyModel dependency, ComponentModel model)
		{
			var key = dependency.DependencyKey;
			if (key == null)
			{
				return null;
			}

			return model.Parameters[key];
		}

		/// <summary>
		///   Extracts the component name from the a ref strings which is
		///   ${something}
		/// </summary>
		/// <param name = "name"></param>
		/// <param name = "keyValue"></param>
		/// <returns></returns>
		protected virtual String ExtractComponentKey(String keyValue, String name)
		{
			if (!ReferenceExpressionUtil.IsReference(keyValue))
			{
				throw new DependencyResolverException(
					String.Format("Key invalid for parameter {0}. " +
					              "Thus the kernel was unable to override the service dependency", name));
			}

			return ReferenceExpressionUtil.ExtractComponentKey(keyValue);
		}

		private void RaiseDependencyResolving(ComponentModel model, DependencyModel dependency, object value)
		{
			dependencyResolvingDelegate(model, dependency, value);
		}

		private bool HasComponentInValidState(string key)
		{
			IHandler handler = kernel.GetHandler(key);

			return IsHandlerInValidState(handler);
		}

		private bool HasComponentInValidState(CreationContext context, Type service)
		{
			IHandler[] handlers = kernel.GetHandlers(service);

			foreach (IHandler handler in handlers)
			{
				if (context == null || handler.IsBeingResolvedInContext(context) == false)
				{
					return IsHandlerInValidState(handler);
				}
			}

			return false;
		}

		private static bool IsHandlerInValidState(IHandler handler)
		{
			if (handler == null)
			{
				return false;
			}

			return handler.CurrentState == HandlerState.Valid;
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
	}
}