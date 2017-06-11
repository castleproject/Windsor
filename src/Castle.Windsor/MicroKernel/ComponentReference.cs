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

namespace Castle.MicroKernel
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Resolvers;

	/// <summary>
	///   Reference to component obtained from the container.
	/// </summary>
	/// <typeparam name = "T"></typeparam>
	[Serializable]
	public class ComponentReference<T> : IReference<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected readonly string referencedComponentName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected readonly Type referencedComponentType;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected DependencyModel dependencyModel;

		/// <summary>
		///   Creates a new instance of <see cref = "ComponentReference{T}" /> referencing default component implemented by <paramref
		///    name = "componentType" />
		/// </summary>
		/// <param name = "componentType"></param>
		public ComponentReference(Type componentType)
		{
			referencedComponentName = ComponentName.DefaultNameFor(componentType);
			referencedComponentType = componentType;
		}

		/// <summary>
		///   Creates a new instance of <see cref = "ComponentReference{T}" /> referencing component <paramref
		///    name = "referencedComponentName" />
		/// </summary>
		/// <param name = "referencedComponentName"></param>
		public ComponentReference(String referencedComponentName)
		{
			if (referencedComponentName == null)
			{
				throw new ArgumentNullException("referencedComponentName");
			}
			this.referencedComponentName = referencedComponentName;
		}

		protected virtual Type ComponentType
		{
			get { return referencedComponentType ?? typeof(T); }
		}

		public T Resolve(IKernel kernel, CreationContext context)
		{
			var handler = GetHandler(kernel);
			if (handler == null)
			{
				throw new DependencyResolverException(
					string.Format(
						"The referenced component {0} could not be resolved. Make sure you didn't misspell the name, and that component is registered.",
						referencedComponentName));
			}

			if (handler.IsBeingResolvedInContext(context))
			{
				throw new DependencyResolverException(
					string.Format(
						"Cycle detected - referenced component {0} wants to use itself as its dependency. This usually signifies a bug in your code.",
						handler.ComponentModel.Name));
			}

			var contextForInterceptor = RebuildContext(handler, context);

			try
			{
				return (T)handler.Resolve(contextForInterceptor);
			}
			catch (InvalidCastException e)
			{
				throw new ComponentResolutionException(
					string.Format("Component {0} is not compatible with type {1}.", referencedComponentName, typeof(T)), e);
			}
		}

		private IHandler GetHandler(IKernel kernel)
		{
			var handler = kernel.GetHandler(referencedComponentName);
			return handler;
		}

		private CreationContext RebuildContext(IHandler handler, CreationContext current)
		{
			var handlerType = ComponentType ?? handler.ComponentModel.Services.First();
			if (handlerType.GetTypeInfo().ContainsGenericParameters)
			{
				return current;
			}

			return new CreationContext(handlerType, current, false);
		}

		void IReference<T>.Attach(ComponentModel component)
		{
			dependencyModel = new ComponentDependencyModel(referencedComponentName, ComponentType);
			component.Dependencies.Add(dependencyModel);
		}

		void IReference<T>.Detach(ComponentModel component)
		{
			if (dependencyModel == null)
			{
				return;
			}
			component.Dependencies.Remove(dependencyModel);
		}
	}
}