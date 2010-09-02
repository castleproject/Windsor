// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel.Context;

	using Core;

	/// <summary>
	/// Handle dependencies of services in the format of typed arrays.
	/// </summary>
	/// <remarks>
	/// This is a complimentary <see cref="ISubDependencyResolver"/> implementation 
	/// that is capable of satisfying dependencies of services as typed arrays.
	/// <para>
	/// Note that it will take precedence over service override for arrays defined 
	/// on the configuration.
	/// </para>
	/// </remarks>
	/// <example>
	/// In order to install the resolver:
	/// <code>
	/// var kernel = new DefaultKernel();
	/// kernel.Resolver.AddSubResolver(new ArrayResolver(kernel));
	/// </code>
	/// 
	/// <para>
	/// To use it, assuming that IService is on the container:
	/// </para>
	/// 
	/// <code>
	/// public class Component
	/// {
	///     public Component(IService[] services)
	///     {
	///     }
	/// }
	/// </code>
	/// </example>
	public class ArrayResolver : ISubDependencyResolver
	{
		private readonly IKernel kernel;
		private readonly bool allowEmptyArray;

		public ArrayResolver(IKernel kernel):this(kernel,false)
		{
		}

		public ArrayResolver(IKernel kernel, bool allowEmptyArray)
		{
			this.kernel = kernel;
			this.allowEmptyArray = allowEmptyArray;
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                      ComponentModel model,
		                      DependencyModel dependency)
		{
			return kernel.ResolveAll(dependency.TargetItemType.GetElementType(), null);
		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                       ComponentModel model,
		                       DependencyModel dependency)
		{
			var targetType = dependency.TargetItemType;
			return targetType != null &&
			       targetType.IsArray &&
			       (allowEmptyArray || kernel.HasComponent(targetType.GetElementType()));
		}
	}
}