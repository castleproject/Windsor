﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	///   Enumeration used to mark the component's lifestyle.
	/// </summary>
	public enum LifestyleType
	{
		/// <summary>
		///   No lifestyle specified.
		/// </summary>
		Undefined = 0,

		/// <summary>
		///   Singleton components are instantiated once, and shared
		///   between all clients.
		/// </summary>
		Singleton = 1,

		/// <summary>
		///   Thread components have a unique instance per thread.
		/// </summary>
		Thread = 2,

		/// <summary>
		///   Transient components are created on demand.
		/// </summary>
		Transient = 3,

		/// <summary>
		///   Optimization of transient components that keeps
		///   instance in a pool instead of always creating them.
		/// </summary>
		Pooled = 4,

		/// <summary>
		///   Any other logic to create/release components.
		/// </summary>
		Custom = 6,

		/// <summary>
		///   Instances are reused within the scope provided.
		/// </summary>
		Scoped = 7,

		/// <summary>
		///   Instance lifetime and reuse scope is bound to another component further up the object graph.
		///   Good scenario for this would be unit of work bound to a presenter in a two tier MVP application.
		///   When specified in xml a <c>scopeRootBinderType</c> attribute must be specified pointing to a type
		///   having default accessible constructor and public method matching signature of <code>Func&lt;IHandler[], IHandler&gt;</code> delegate.
		/// </summary>
		Bound = 8
	}
}