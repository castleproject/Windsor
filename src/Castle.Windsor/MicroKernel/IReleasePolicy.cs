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

	using Castle.Core;

	/// <summary>
	///   Policy managing lifetime of components, and in particular their release process.
	/// </summary>
	public interface IReleasePolicy : IDisposable
	{
		/// <summary>
		/// Creates nested policy for scoped object tracking.
		/// </summary>
		/// <returns></returns>
		IReleasePolicy CreateSubPolicy();

		/// <summary>
		/// Returns <c>true</c> if <paramref name="instance"/> is being tracked by this release policy, <c>false</c> otherwise.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		bool HasTrack(object instance);

		/// <summary>
		/// Releases <paramref name="instance"/> if it's being tracked by this release policy, invoking its <see cref="IDecommissionConcern"/>s and stops tracking it so that it can be claimed by the Garbage Collector.
		/// </summary>
		/// <param name="instance"></param>
		void Release(object instance);

		/// <summary>
		///   Asks the policy to track given object. The object will be released when a call to <see cref = "Release" /> is made.
		/// </summary>
		/// <param name = "instance"></param>
		/// <param name = "burden"></param>
		/// <exception cref = "ArgumentException">Thrown when <paramref name = "burden" /> does NOT have its <see
		///    cref = "Burden.RequiresPolicyRelease" /> flag set.</exception>
		void Track(object instance, Burden burden);
	}
}