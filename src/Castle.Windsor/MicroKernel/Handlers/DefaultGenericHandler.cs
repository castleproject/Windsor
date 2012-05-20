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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;

	[Serializable]
	public class DefaultGenericHandler : AbstractHandler
	{
		private readonly IGenericImplementationMatchingStrategy implementationMatchingStrategy;

		private readonly SimpleThreadSafeDictionary<Type, IHandler> type2SubHandler = new SimpleThreadSafeDictionary<Type, IHandler>();

		/// <summary>
		///   Initializes a new instance of the <see cref="DefaultGenericHandler" /> class.
		/// </summary>
		/// <param name="model"> </param>
		/// <param name="implementationMatchingStrategy"> </param>
		public DefaultGenericHandler(ComponentModel model, IGenericImplementationMatchingStrategy implementationMatchingStrategy) : base(model)
		{
			this.implementationMatchingStrategy = implementationMatchingStrategy;
		}

		public IGenericImplementationMatchingStrategy ImplementationMatchingStrategy
		{
			get { return implementationMatchingStrategy; }
		}

		public override void Dispose()
		{
			type2SubHandler.Dispose();
		}

		public override bool ReleaseCore(Burden burden)
		{
			var genericType = ProxyUtil.GetUnproxiedType(burden.Instance);

			var handler = GetSubHandler(CreationContext.CreateEmpty(), genericType);

			return handler.Release(burden);
		}

		public override bool Supports(Type service)
		{
			if (base.Supports(service))
			{
				return true;
			}
			if (service.IsGenericType && service.IsGenericTypeDefinition == false)
			{
				var openService = service.GetGenericTypeDefinition();
				return base.Supports(openService);
			}
			return false;
		}

		protected virtual Type[] AdaptServices(CreationContext context, Type closedImplementationType)
		{
			var services = ComponentModel.Services.ToArray();
			if (services.Length == 1 && services[0] == context.RequestedType.GetGenericTypeDefinition())
			{
				// shortcut for the most common case
				return new[] {context.RequestedType};
			}
			var adaptedServices = new List<Type>(services.Length);
			var index = 0;
			// we split the check into two parts: first we inspect class services...
			var genericDefinitionToClass = default(IDictionary<Type, Type>);
			while (index < services.Length && services[index].IsClass)
			{
				var service = services[index];
				if (service.IsGenericTypeDefinition)
				{
					EnsureClassMappingInitialized(closedImplementationType, ref genericDefinitionToClass);
					Type closed;
					if (genericDefinitionToClass.TryGetValue(service, out closed))
					{
						adaptedServices.Add(closed);
					}
					else
					{
						// NOTE: it's an interface not exposed by the implementation type. Possibly aimed at a proxy... I guess we can ignore it for now. Don't have any better idea.
						Debug.Fail(string.Format("Could not find mapping for interface {0} on implementation type {1}", service, closedImplementationType));
					}
				}
				else
				{
					adaptedServices.Add(service);
				}
				index++;
			}
			if (index == (services.Length - 1))
			{
				return adaptedServices.ToArray();
			}
			var genericDefinitionToInterface = closedImplementationType.GetInterfaces()
				.Where(i => i.IsGenericType)
				.ToDictionary(i => i.GetGenericTypeDefinition());
			while (index < services.Length)
			{
				var service = services[index];
				if (service.IsGenericTypeDefinition)
				{
					Type closed;
					if (genericDefinitionToInterface.TryGetValue(service, out closed))
					{
						adaptedServices.Add(closed);
					}
					else
					{
						// NOTE: it's an interface not exposed by the implementation type. Possibly aimed at a proxy... I guess we can ignore it for now. Don't have any better idea.
						Debug.Fail(string.Format("Could not find mapping for interface {0} on implementation type {1}", service, closedImplementationType));
					}
				}
				else
				{
					adaptedServices.Add(service);
				}
				index++;
			}
			if (adaptedServices.Count == 0)
			{
				// we obviously have either a bug or an uncovered case. I suppose the best we can do at this point is to fallback to the old behaviour
				return new[] {context.RequestedType};
			}
			return adaptedServices.ToArray();
		}

		protected virtual IHandler BuildSubHandler(CreationContext context, Type closedImplementationType)
		{
			// TODO: we should probably match the requested type to existing services and close them over its generic arguments
			var newModel = Kernel.ComponentModelBuilder.BuildModel(
				ComponentModel.ComponentName,
				AdaptServices(context, closedImplementationType),
				closedImplementationType,
				GetExtendedProperties());
			CloneParentProperties(newModel);
			// Create the handler and add to type2SubHandler before we add to the kernel.
			// Adding to the kernel could satisfy other dependencies and cause this method
			// to be called again which would result in extra instances being created.
			return Kernel.AddCustomComponent(newModel, isMetaHandler: true);
		}

		protected IHandler GetSubHandler(CreationContext context, Type genericType)
		{
			return type2SubHandler.GetOrAdd(genericType, t => BuildSubHandler(context, t));
		}

		protected override void InitDependencies()
		{
			// not too convinved we need to support that in here but let's be safe...
			var activator = Kernel.CreateComponentActivator(ComponentModel) as IDependencyAwareActivator;
			if (activator != null && activator.CanProvideRequiredDependencies(ComponentModel))
			{
				return;
			}

			base.InitDependencies();
		}

		protected override object Resolve(CreationContext context, bool instanceRequired)
		{
			var implType = GetClosedImplementationType(context, instanceRequired);
			if (implType == null)
			{
				Debug.Assert(instanceRequired == false, "instanceRequired == false");
				return null;
			}

			var handler = GetSubHandler(context, implType);
			// so the generic version wouldn't be considered as well
			using (context.EnterResolutionContext(this, false, false))
			{
				try
				{
					return handler.Resolve(context);
				}
				catch (GenericHandlerTypeMismatchException e)
				{
					throw new HandlerException(
						string.Format(
							"Generic component {0} has some generic dependencies which were not successfully closed. This often happens when generic implementation has some additional generic constraints. See inner exception for more details.",
							ComponentModel.Name), ComponentModel.ComponentName, e);
				}
			}
		}

		///<summary>
		///  Clone some of the parent componentmodel properties to the generic subhandler.
		///</summary>
		///<remarks>
		///  The following properties are copied: <list type="bullet">
		///                                         <item>
		///                                           <description>The
		///                                             <see cref="LifestyleType" />
		///                                           </description>
		///                                         </item>
		///                                         <item>
		///                                           <description>The
		///                                             <see cref="ComponentModel.Interceptors" />
		///                                           </description>
		///                                         </item>
		///                                       </list>
		///</remarks>
		///<param name="newModel"> the subhandler </param>
		private void CloneParentProperties(ComponentModel newModel)
		{
			// Inherits from LifeStyle's context.
			newModel.LifestyleType = ComponentModel.LifestyleType;

			// Inherit the parent handler interceptors.
			foreach (InterceptorReference interceptor in ComponentModel.Interceptors)
			{
				// we need to check that we are not adding the inteceptor again, if it was added
				// by a facility already
				newModel.Interceptors.AddIfNotInCollection(interceptor);
			}

			if (ComponentModel.HasCustomDependencies)
			{
				var dependencies = newModel.CustomDependencies;
				foreach (DictionaryEntry dependency in ComponentModel.CustomDependencies)
				{
					dependencies.Add(dependency.Key, dependency.Value);
				}
			}
		}

		private Type GetClosedImplementationType(CreationContext context, bool instanceRequired)
		{
			var genericArguments = GetGenericArguments(context);
			try
			{
				// TODO: what if ComponentModel.Implementation is a LateBoundComponent?
				return ComponentModel.Implementation.MakeGenericType(genericArguments);
			}
			catch (ArgumentNullException)
			{
				if (implementationMatchingStrategy == null)
				{
					// NOTE: if we're here something is badly screwed...
					throw;
				}
				throw new HandlerException(
					string.Format(
						"Custom {0} ({1}) didn't select any generic parameters for implementation type of component '{2}'. This usually signifies bug in the {0}.",
						typeof (IGenericImplementationMatchingStrategy).Name, implementationMatchingStrategy, ComponentModel.Name), ComponentModel.ComponentName);
			}
			catch (ArgumentException e)
			{
				// may throw in some cases when impl has generic constraints that service hasn't
				if (instanceRequired == false)
				{
					return null;
				}

				// ok, let's do some investigation now what might have been the cause of the error
				// there can be 3 reasons according to MSDN: http://msdn.microsoft.com/en-us/library/system.type.makegenerictype.aspx

				var arguments = ComponentModel.Implementation.GetGenericArguments();
				string message;
				// 1.
				if (arguments.Length > genericArguments.Length)
				{
					message = string.Format(
						"Requested type {0} has {1} generic parameter(s), whereas component implementation type {2} requires {3}. This means that Windsor does not have enough information to properly create that component for you. This is most likely a bug in your registration code.",
						context.RequestedType,
						context.GenericArguments.Length,
						ComponentModel.Implementation,
						arguments.Length);
					throw new HandlerException(message, ComponentModel.ComponentName, e);
				}

				// 2.
				var invalidArguments = genericArguments.Where(a => a.IsPointer || a.IsByRef || a == typeof (void)).Select(t => t.FullName).ToArray();
				if (invalidArguments.Length > 0)
				{
					message = string.Format("The following types provided as generic parameters are not legal: {0}. This is most likely a bug in your code.",
					                        string.Join(", ", invalidArguments));
					throw new HandlerException(message, ComponentModel.ComponentName, e);
				}
				// 3. at this point we should be 99% sure we have arguments that don't satisfy generic constraints of out service.
				throw new GenericHandlerTypeMismatchException(genericArguments, ComponentModel, this);
			}
		}

		private IDictionary GetExtendedProperties()
		{
			var extendedProperties = ComponentModel.ExtendedProperties;
			if (extendedProperties != null && extendedProperties.Count > 0)
			{
#if !SILVERLIGHT
				if (extendedProperties is ICloneable)
				{
					extendedProperties = (IDictionary) ((ICloneable) extendedProperties).Clone();
				}
#endif
				extendedProperties = new Arguments(extendedProperties);
			}
			return extendedProperties;
		}

		private Type[] GetGenericArguments(CreationContext context)
		{
			if (implementationMatchingStrategy == null)
			{
				return context.GenericArguments;
			}
			return implementationMatchingStrategy.GetGenericArguments(ComponentModel, context) ?? context.GenericArguments;
		}

		private static void EnsureClassMappingInitialized(Type closedImplementationType, ref IDictionary<Type, Type> genericDefinitionToClass)
		{
			if (genericDefinitionToClass == null)
			{
				genericDefinitionToClass = new Dictionary<Type, Type>();
				var type = closedImplementationType;
				while (type != typeof (object))
				{
					if (type.IsGenericType)
					{
						genericDefinitionToClass.Add(type.GetGenericTypeDefinition(), type);
					}
					type = type.BaseType;
				}
			}
		}
	}
}