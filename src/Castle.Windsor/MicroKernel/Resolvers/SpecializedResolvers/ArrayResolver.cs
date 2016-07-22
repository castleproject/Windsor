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

	/// <summary>
	///   Handle dependencies of services in the format of typed arrays.
	/// </summary>
	/// <remarks>
	///   This is a complimentary <see cref = "ISubDependencyResolver" /> implementation 
	///   that is capable of satisfying dependencies of services as typed arrays.
	///   <para>
	///     Note that it will take precedence over service override for arrays defined 
	///     on the configuration.
	///   </para>
	/// </remarks>
	/// <example>
	///   In order to install the resolver:
	///   <code>
	///     var kernel = new DefaultKernel();
	///     kernel.Resolver.AddSubResolver(new ArrayResolver(kernel));
	///   </code>
	/// 
	///   <para>
	///     To use it, assuming that IService is on the container:
	///   </para>
	/// 
	///   <code>
	///     public class Component
	///     {
	///     public Component(IService[] services)
	///     {
	///     }
	///     }
	///   </code>
	/// </example>
	public class ArrayResolver : CollectionResolver
	{
		public ArrayResolver(IKernel kernel)
			: base(kernel, false)
		{
		}

		public ArrayResolver(IKernel kernel, bool allowEmptyArray)
			: base(kernel, allowEmptyArray)
		{
		}

		protected override Type GetItemType(Type targetItemType)
		{
			if (targetItemType.IsArray)
			{
				return targetItemType.GetElementType();
			}
			return null;
		}
	}
}