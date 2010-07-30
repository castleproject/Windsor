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

namespace Castle.MicroKernel.Resolvers.SpecializedResolvers
{
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;

	/// <summary>
	/// More generic alternative to <see cref="ArrayResolver"/> and <see cref="ListResolver"/>.
	/// It supports arrays as well as any generic interface type assignable from arrays.
	/// </summary>
	/// <remarks>
	/// The collection instance that is provided is read only, even for interfaces like <see cref="IList{T}"/>
	/// </remarks>
	public class CollectionResolver : ISubDependencyResolver
	{
		private readonly bool allowEmptyCollections;
		private readonly IKernel kernel;

		public CollectionResolver(IKernel kernel, bool allowEmptyCollections = false)
		{
			this.kernel = kernel;
			this.allowEmptyCollections = allowEmptyCollections;
		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                       DependencyModel dependency)
		{
			if (dependency.TargetType == null)
			{
				return false;
			}

			var itemType = dependency.TargetType.GetCompatibileArrayItemType();
			if (itemType == null)
			{
				return false;
			}

			return allowEmptyCollections || kernel.HasComponent(itemType);
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                      DependencyModel dependency)
		{
			return kernel.ResolveAll(dependency.TargetType.GetCompatibileArrayItemType(), null);
		}
	}
}