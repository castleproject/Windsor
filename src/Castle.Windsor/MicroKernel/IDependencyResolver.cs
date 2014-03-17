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
	/// <summary>
	///   Implementors should use a strategy to obtain 
	///   valid references to properties and/or services 
	///   requested in the dependency model.
	/// </summary>
	public interface IDependencyResolver : ISubDependencyResolver
	{
		/// <summary>
		///   Registers a sub resolver instance
		/// </summary>
		/// <param name = "subResolver">The subresolver instance</param>
		void AddSubResolver(ISubDependencyResolver subResolver);

		/// <summary>
		///   This method is called with a delegate for firing the
		///   IKernelEvents.DependencyResolving event.
		/// </summary>
		/// <param name = "kernel">kernel</param>
		/// <param name = "resolving">The delegate used to fire the event</param>
		void Initialize(IKernelInternal kernel, DependencyDelegate resolving);

		/// <summary>
		///   Unregisters a sub resolver instance previously registered
		/// </summary>
		/// <param name = "subResolver">The subresolver instance</param>
		void RemoveSubResolver(ISubDependencyResolver subResolver);
	}
}