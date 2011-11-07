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

namespace Castle.Core
{
	/// <summary>
	///   Specifies rules for designating settable properties on a component as dependencies, and controlling whether they are requred or not.
	///   This is a shortcut for most common scenarios. More advanced/custom scenarios can be defined dynamically in the registration API.
	/// </summary>
	public enum PropertyFilter
	{
		/// <summary>
		///   Takes no action. By default that means all settable properties will be exposed as optional dependencies.
		/// </summary>
		Default,

		/// <summary>
		///   Makes all property dependencies required.
		/// </summary>
		RequireAll,

		/// <summary>
		///   Makes all property dependencies defined at a base class/interfaces level required.
		/// </summary>
		RequireBase,

		/// <summary>
		///   Makes all properties ignored.
		/// </summary>
		IgnoreAll,

		/// <summary>
		///   Ignores all properties defined at a base class/interface level.
		/// </summary>
		/// <remarks>
		///   This option is particularily useful in scenarios like UI controls which in .NET UI frameworks tend to have byzantine inheritance hierarchies.
		/// </remarks>
		IgnoreBase,
	}
}