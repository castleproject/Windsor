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

namespace Castle.MicroKernel.ComponentActivator
{
	using Castle.Core;

	/// <summary>
	/// Implemented by <see cref="IComponentActivator"/> which don't necessarily need dependencies from the container to activate new instances of the component.
	/// </summary>
	public interface IDependencyAwareActivator
	{
		/// <summary>
		/// Should return <c>true</c> if the activator can provide dependencies for the <paramref name="component"/>.
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		bool CanProvideRequiredDependencies(ComponentModel component);

		/// <summary>
		/// Should return <c>true</c> if the activated instances of the <see cref="component"/> are managed externally to the container. That means container will not try to track the objects in <see cref="IReleasePolicy"/>.
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		bool IsManagedExternally(ComponentModel component);
	}
}