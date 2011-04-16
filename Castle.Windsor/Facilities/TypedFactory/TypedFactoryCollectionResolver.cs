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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Collections;

	using Castle.MicroKernel;

	/// <summary>
	///   Represents a set of components to be resolved via Typed Factory. Uses <see
	///    cref = "IKernel.ResolveAll(System.Type,System.Collections.IDictionary)" /> to resolve the components.
	/// </summary>
	public class TypedFactoryCollectionResolver : ITypedFactoryComponentResolver
	{
		private readonly IDictionary additionalArguments;
		private readonly Type componentCollectionType;

		/// <summary>
		///   Creates new instance of <see cref = "TypedFactoryCollectionResolver" />.
		/// </summary>
		/// <param name = "componentCollectionType">Collection type to resolve. Must be an array (SomeComponent[]) or IEnumerable{SomeComponent}. Type of the element of the collection will be used as first argument to <see
		///    cref = "IKernel.ResolveAll(System.Type,System.Collections.IDictionary)" /></param>
		/// <param name = "additionalArguments">Additional arguents that will be passed as second argument to <see
		///    cref = "IKernel.ResolveAll(System.Type,System.Collections.IDictionary)" /></param>
		public TypedFactoryCollectionResolver(Type componentCollectionType, IDictionary additionalArguments)
		{
			this.componentCollectionType = componentCollectionType;
			this.additionalArguments = additionalArguments;
		}

		public object Resolve(IKernelInternal kernel, IReleasePolicy scope)
		{
			var result = kernel.ResolveAll(componentCollectionType, additionalArguments, scope);
			return result;
		}
	}
}