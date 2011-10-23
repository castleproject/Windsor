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

	/// <summary>
	///   Specifies default configuration for a typed factory. All Selector* properties are mutually exclusive, that is you're only meant to set one.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public class FactoryAttribute : Attribute
	{
		/// <summary>
		///   Specifies component to use as selector for given factory. This works like any named service override.
		/// </summary>
		public string SelectorComponentName { get; set; }

		/// <summary>
		///   Specifies component to use as selector for given factory. This works like any typed service override.
		/// </summary>
		public Type SelectorComponentType { get; set; }

		/// <summary>
		///   Specifies type of the selector to use for given factory. The type will be instantiated using default constructor. It must implement <see
		///    cref = "ITypedFactoryComponentSelector" />
		/// </summary>
		public Type SelectorType { get; set; }
	}
}