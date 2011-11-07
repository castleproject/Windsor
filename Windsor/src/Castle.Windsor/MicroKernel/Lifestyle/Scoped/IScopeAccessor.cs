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

namespace Castle.MicroKernel.Lifestyle.Scoped
{
	using System;

	using Castle.MicroKernel.Context;

	/// <summary>
	///   Provides access to <see cref = "IScopeCache" /> held in whatever is appropriate for given scope.
	/// </summary>
	/// <remarks>
	///   Implementors should also ensure proper initialization of <see cref = "IScopeCache" /> when accessed for the first time and ensure a thread safe implementation is used when scope or cache access can cause threading issues if non thread safe cache is used.
	/// </remarks>
	public interface IScopeAccessor : IDisposable
	{
		/// <summary>
		///   Provides access to <see cref = "IScopeCache" /> for currently resolved component.
		/// </summary>
		/// <param name = "context">Current creation context</param>
		/// <exception cref = "T:System.InvalidOperationException"> Thrown when scope cache could not be accessed.</exception>
		ILifetimeScope GetScope(CreationContext context);
	}
}