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

namespace Castle.Core
{
	using System;
	using System.Diagnostics;
	using System.Text;

	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Resolvers;

	/// <summary>
	///   Represents an reference to a Interceptor component.
	/// </summary>
	[Serializable]
	public class InterceptorReference : IReference<IInterceptor>, IEquatable<InterceptorReference>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string referencedComponentName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Type referencedComponentType;

		/// <summary>
		///   Initializes a new instance of the <see cref = "InterceptorReference" /> class.
		/// </summary>
		/// <param name = "referencedComponentName">The component key.</param>
		public InterceptorReference(String referencedComponentName)
		{
			if (referencedComponentName == null)
			{
				throw new ArgumentNullException("referencedComponentName");
			}
			this.referencedComponentName = referencedComponentName;
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "InterceptorReference" /> class.
		/// </summary>
		/// <param name = "componentType">Type of the interceptor to use. This will reference the default component (ie. one with no explicitly assigned name) implemented by given type.</param>
		public InterceptorReference(Type componentType)
		{
			if (componentType == null)
			{
				throw new ArgumentNullException("componentType");
			}
			referencedComponentName = ComponentName.DefaultNameFor(componentType);
			referencedComponentType = componentType;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals(obj as InterceptorReference);
		}

		public override int GetHashCode()
		{
			return referencedComponentName.GetHashCode();
		}

		public override string ToString()
		{
			return referencedComponentName;
		}

		public bool Equals(InterceptorReference other)
		{
			if (other == null)
			{
				return false;
			}
			return Equals(referencedComponentName, other.referencedComponentName);
		}

		private Type ComponentType()
		{
			return referencedComponentType ?? typeof(IInterceptor);
		}

		private StringBuilder GetExceptionMessageOnHandlerNotFound(IKernel kernel)
		{
			var message = new StringBuilder(string.Format("The interceptor '{0}' could not be resolved. ", referencedComponentName));
			// ok so the component is missing. Now - is it missing because it's not been registered or because the reference is by type and interceptor was registered with custom name?
			if (referencedComponentType != null)
			{
				var typedHandler = kernel.GetHandler(referencedComponentType);
				if (typedHandler != null)
				{
					message.AppendFormat(
						"Component '{0}' matching the type specified was found in the container. Did you mean to use it? If so, please specify the reference via name, or register the interceptor without specifying its name.",
						typedHandler.ComponentModel.Name);
					return message;
				}
			}
			message.Append("Did you forget to register it?");
			return message;
		}

		private IHandler GetInterceptorHandler(IKernel kernel)
		{
			if (referencedComponentType != null)
			{
				//try old behavior first
				var handler = kernel.GetHandler(referencedComponentType.FullName);
				if (handler != null)
				{
					return handler;
				}
				// new bahavior as a fallback
				return kernel.GetHandler(referencedComponentType);
			}

			return kernel.GetHandler(referencedComponentName);
		}

		private CreationContext RebuildContext(Type handlerType, CreationContext current)
		{
			if (handlerType.ContainsGenericParameters)
			{
				return current;
			}

			return new CreationContext(handlerType, current, true);
		}

		void IReference<IInterceptor>.Attach(ComponentModel component)
		{
			component.Dependencies.Add(new ComponentDependencyModel(referencedComponentName, ComponentType()));
		}

		void IReference<IInterceptor>.Detach(ComponentModel component)
		{
			throw new NotSupportedException();
		}

		IInterceptor IReference<IInterceptor>.Resolve(IKernel kernel, CreationContext context)
		{
			var handler = GetInterceptorHandler(kernel);
			if (handler == null)
			{
				var message = GetExceptionMessageOnHandlerNotFound(kernel);
				throw new DependencyResolverException(message.ToString());
			}

			if (handler.IsBeingResolvedInContext(context))
			{
				throw new DependencyResolverException(
					string.Format(
						"Cycle detected - interceptor {0} wants to use itself as its interceptor. This usually signifies a bug in custom {1}",
						handler.ComponentModel.Name, typeof(IModelInterceptorsSelector).Name));
			}

			var contextForInterceptor = RebuildContext(ComponentType(), context);
			return (IInterceptor)handler.Resolve(contextForInterceptor);
		}

		/// <summary>
		///   Gets an <see cref = "InterceptorReference" /> for the component key.
		/// </summary>
		/// <param name = "key">The component key.</param>
		/// <returns>The <see cref = "InterceptorReference" /></returns>
		public static InterceptorReference ForKey(String key)
		{
			return new InterceptorReference(key);
		}

		/// <summary>
		///   Gets an <see cref = "InterceptorReference" /> for the service.
		/// </summary>
		/// <param name = "service">The service.</param>
		/// <returns>The <see cref = "InterceptorReference" /></returns>
		public static InterceptorReference ForType(Type service)
		{
			return new InterceptorReference(service);
		}

		/// <summary>
		///   Gets an <see cref = "InterceptorReference" /> for the service.
		/// </summary>
		/// <typeparam name = "T">The service type.</typeparam>
		/// <returns>The <see cref = "InterceptorReference" /></returns>
		public static InterceptorReference ForType<T>()
		{
			return new InterceptorReference(typeof(T));
		}
	}
}