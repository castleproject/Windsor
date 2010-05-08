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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.MicroKernel;

	/// <summary>
	/// Represents a set of components to be resolved via Typed Factory. Uses <see cref="IKernel.ResolveAll(System.Type,System.Collections.IDictionary)"/> to resolve the components.
	/// </summary>
	public class TypedFactoryComponentCollection : TypedFactoryComponent
	{
		/// <summary>
		/// Creates new instance of <see cref="TypedFactoryComponentCollection"/>.
		/// </summary>
		/// <param name="componentCollectionType">Collection type to resolve. Must be an array (SomeComponent[]) or IEnumerable{SomeComponent}. Type of the element of the collection will be used as first argument to <see cref="IKernel.ResolveAll(System.Type,System.Collections.IDictionary)"/></param>
		/// <param name="additionalArguments">Additional arguents that will be passed as second argument to <see cref="IKernel.ResolveAll(System.Type,System.Collections.IDictionary)"/></param>
		public TypedFactoryComponentCollection(Type componentCollectionType, IDictionary additionalArguments)
			: base(null, componentCollectionType, additionalArguments)
		{
		}

		public override object Resolve(IKernel kernel)
		{
			var service = GetCollectionItemType();
			var result = kernel.ResolveAll(service, AdditionalArguments);

			if (service == typeof(object))
			{
				return result;
			}

			var array = Array.CreateInstance(service, result.Length);
			result.CopyTo(array, 0);
			return array;
		}

		protected Type GetCollectionItemType()
		{
			if (ComponentType.IsArray)
			{
				return ComponentType.GetElementType();
			}

			if (ComponentType.IsGenericType) // we support generic collections only
			{
				foreach (var @interface in TypeUtil.GetAllInterfaces(ComponentType))
				{
					if (@interface.IsGenericType == false)
					{
						continue;
					}

					if (@interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					{
						return @interface.GetGenericArguments().Single();
					}
				}
			}

			ThrowUnsupportedCollectionType();
			return null; //to satify the compiler
		}

		private void ThrowUnsupportedCollectionType()
		{
			throw new InvalidOperationException(
				string.Format(
					"Type {0} is not supported collection type. If you want to support it, your ITypedFactoryComponentSelector implementation should return custom TypedFactoryComponent that will appropriately override Resolve method.",
					ComponentType));
		}

		public static bool IsSupportedCollectionType(Type type)
		{
			if (type.IsArray)
			{
				return true;
			}
			return type.IsGenericType &&
			       TypeUtil.GetAllInterfaces(type)
			       	.Where(@interface => @interface.IsGenericType)
			       	.Any(@interface => @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}
	}
}