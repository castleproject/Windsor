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
	using System.ComponentModel;

	public partial interface IKernel : IKernelEvents, IDisposable
	{
		/// <summary>
		///   Adds a <see cref = "IFacility" /> to the kernel.
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "facility"></param>
		/// <returns></returns>
		[Obsolete("Use AddFacility(IFacility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IKernel AddFacility(String key, IFacility facility);

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the kernel.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "key"></param>
		[Obsolete("Use AddFacility<TFacility>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IKernel AddFacility<T>(String key) where T : IFacility, new();

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the kernel.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "key"></param>
		/// <param name = "onCreate">The callback for creation.</param>
		[Obsolete("Use AddFacility<TFacility>(Action<TFacility>) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IKernel AddFacility<T>(String key, Action<T> onCreate)
			where T : IFacility, new();
	}
}