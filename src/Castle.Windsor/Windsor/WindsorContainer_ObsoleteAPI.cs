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

namespace Castle.Windsor
{
	using System;
	using System.ComponentModel;

	using Castle.MicroKernel;

	public partial class WindsorContainer
	{
		/// <summary>
		///   Registers a facility within the container.
		/// </summary>
		/// <param name = "idInConfiguration"></param>
		/// <param name = "facility"></param>
		[Obsolete("Use AddFacility(IFacility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IWindsorContainer AddFacility(String idInConfiguration, IFacility facility)
		{
			kernel.AddFacility(idInConfiguration, facility);
			return this;
		}

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "idInConfiguration"></param>
		/// <returns></returns>
		[Obsolete("Use AddFacility<TFacility>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddFacility<T>(String idInConfiguration) where T : IFacility, new()
		{
			kernel.AddFacility<T>(idInConfiguration);
			return this;
		}

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "idInConfiguration"></param>
		/// <param name = "configureFacility">The callback for creation.</param>
		/// <returns></returns>
		[Obsolete("Use AddFacility<TFacility>(Action<TFacility>) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddFacility<T>(String idInConfiguration, Action<T> configureFacility)
			where T : IFacility, new()
		{
			kernel.AddFacility(idInConfiguration, configureFacility);
			return this;
		}
	}
}