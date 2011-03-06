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

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Util;

	/// <summary>
	///   Reference to component obtained from a container.
	/// </summary>
	/// <typeparam name = "T"></typeparam>
	public class ComponentReference<T> : IReference<T>
	{
		private readonly Type actualComponentType;
		private readonly DependencyModel dependency;
		private readonly string serviceOverrideComponentKey;

		public ComponentReference(string dependencyName, string componentKey)
		{
			if (dependencyName == null)
			{
				throw new ArgumentNullException("dependencyName");
			}
			if (componentKey == null)
			{
				throw new ArgumentNullException("componentKey");
			}

			serviceOverrideComponentKey = componentKey;

			dependency = new DependencyModel(dependencyName, typeof(T), false);
		}

		public ComponentReference(string dependencyName, Type actualComponentType)
		{
			if (actualComponentType == null)
			{
				throw new ArgumentNullException("actualComponentType");
			}
			this.actualComponentType = actualComponentType;

			dependency = new DependencyModel(dependencyName, actualComponentType, false);
		}

		public ComponentReference(string dependencyName)
			: this(dependencyName, typeof(T))
		{
			// so that we don't have to specify the key
		}

		public void Attach(ComponentModel component)
		{
			var reference = ReferenceExpressionUtil.BuildReference(serviceOverrideComponentKey ?? actualComponentType.FullName);
			component.Parameters.Add(dependency.DependencyKey, reference);
			component.Dependencies.Add(dependency);
		}

		public void Detach(ComponentModel component)
		{
			component.Dependencies.Remove(dependency);
		}

		public T Resolve(IKernel kernel, CreationContext context)
		{
			var handler = GetHandler(kernel);
			if (handler == null)
			{
				throw new Exception(
					string.Format(
						"Component {0} could not be resolved. Make sure you didn't misspell the name, and that component is registered.",
						serviceOverrideComponentKey ?? actualComponentType.ToString()));
			}

			try
			{
				return (T)handler.Resolve(context);
			}
			catch (InvalidCastException e)
			{
				throw new Exception(string.Format("Component {0} is not compatible with type {1}.", serviceOverrideComponentKey, typeof(T)), e);
			}
		}

		private IHandler GetHandler(IKernel kernel)
		{
			if (serviceOverrideComponentKey != null)
			{
				return kernel.GetHandler(serviceOverrideComponentKey);
			}

			return kernel.GetHandler(actualComponentType);
		}
	}
}