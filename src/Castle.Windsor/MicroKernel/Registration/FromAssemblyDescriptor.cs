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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core.Internal;

	/// <summary>
	///   Selects a set of types from an assembly.
	/// </summary>
	public class FromAssemblyDescriptor : FromDescriptor
	{
		protected readonly IEnumerable<Assembly> assemblies;
		protected bool nonPublicTypes;

		protected internal FromAssemblyDescriptor(Assembly assembly, Predicate<Type> additionalFilters) : base(additionalFilters)
		{
			assemblies = new[] { assembly };
		}

		protected internal FromAssemblyDescriptor(IEnumerable<Assembly> assemblies, Predicate<Type> additionalFilters)
			: base(additionalFilters)
		{
			this.assemblies = assemblies;
		}

		/// <summary>
		///   When called also non-public types will be scanned.
		/// </summary>
		/// <remarks>
		///   Usually it is not recommended to register non-public types in the container so think twice before using this option.
		/// </remarks>
		public FromAssemblyDescriptor IncludeNonPublicTypes()
		{
			nonPublicTypes = true;
			return this;
		}

		protected override IEnumerable<Type> SelectedTypes(IKernel kernel)
		{
			return assemblies.SelectMany(a => a.GetAvailableTypesOrdered(nonPublicTypes));
		}
	}
}