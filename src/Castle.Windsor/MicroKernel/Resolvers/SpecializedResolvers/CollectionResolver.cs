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

namespace Castle.MicroKernel.Resolvers.SpecializedResolvers
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   More generic alternative to <see cref = "ArrayResolver" /> and <see cref = "ListResolver" />.
	///   It supports arrays as well as any generic interface type assignable from arrays.
	/// </summary>
	/// <remarks>
	///   The collection instance that is provided is read only, even for interfaces like <see cref = "IList{T}" />
	/// </remarks>
	public class CollectionResolver : ISubDependencyResolver
	{
		protected readonly bool allowEmptyCollections;
		protected readonly IKernel kernel;

		public CollectionResolver(IKernel kernel, bool allowEmptyCollections = false)
		{
			this.kernel = kernel;
			this.allowEmptyCollections = allowEmptyCollections;
		}

		public virtual bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                               ComponentModel model,
		                               DependencyModel dependency)
		{
			if (dependency.TargetItemType == null)
			{
				return false;
			}

			var itemType = GetItemType(dependency.TargetItemType);
			return itemType != null &&
			       HasParameter(dependency) == false &&
			       CanSatisfy(itemType);
		}

		public virtual object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                              DependencyModel dependency)
		{
			return kernel.ResolveAll(GetItemType(dependency.TargetItemType), context.AdditionalArguments);
		}

		protected virtual bool CanSatisfy(Type itemType)
		{
			return allowEmptyCollections || kernel.HasComponent(itemType);
		}

		protected virtual Type GetItemType(Type targetItemType)
		{
			return targetItemType.GetCompatibleArrayItemType();
		}

		protected virtual bool HasParameter(DependencyModel dependency)
		{
			return dependency.Parameter != null;
		}
	}
}
