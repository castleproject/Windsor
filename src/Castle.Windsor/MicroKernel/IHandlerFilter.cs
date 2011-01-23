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

namespace Castle.MicroKernel
{
	using System;

	/// <summary>
	///   Implementation of this interface allows for extension of the way
	///   the container looks up multiple handlers. It provides the necessary
	///   hook to allow for business logic to prioritize, filter, and sort
	///   handlers when resolving multiple handlers.
	/// </summary>
	public interface IHandlerFilter
	{
		/// <summary>
		///   Whatever the selector has an opinion about resolving a component with the 
		///   specified service and key.
		/// </summary>
		/// <param name = "service">The service interface that we want to resolve</param>
		bool HasOpinionAbout(Type service);

		/// <summary>
		///   Select the appropriate handlers (if any) from the list of defined handlers,
		///   returning them in the order they should be executed.
		///   The returned handlers should members from the <paramref name = "handlers" /> array.
		/// </summary>
		/// <param name = "service">The service interface that we want to resolve</param>
		/// <param name = "handlers">The defined handlers</param>
		/// <returns>The selected handlers, or an empty array, or null</returns>
		IHandler[] SelectHandlers(Type service, IHandler[] handlers);
	}
}