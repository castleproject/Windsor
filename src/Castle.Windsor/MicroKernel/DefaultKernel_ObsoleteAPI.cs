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
	using System.Linq;

	public partial class DefaultKernel
	{
		[Obsolete("Use AddFacility(IFacility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IKernel AddFacility(String key, IFacility facility)
		{
			if (facility == null)
			{
				throw new ArgumentNullException("facility");
			}
			var facilityType = facility.GetType();
			if (facilities.Any(f => f.GetType() == facilityType))
			{
				throw new ArgumentException(
					string.Format(
						"Facility of type '{0}' has already been registered with the container. Only one facility of a given type can exist in the container.",
						facilityType.FullName));
			}
			facilities.Add(facility);
			facility.Init(this, ConfigurationStore.GetFacilityConfiguration(key));

			return this;
		}

		[Obsolete("Use AddFacility<TFacility>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IKernel AddFacility<T>(String key) where T : IFacility, new()
		{
			return AddFacility(new T());
		}

		[Obsolete("Use AddFacility<TFacility>(Action<TFacility>) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IKernel AddFacility<T>(String key, Action<T> onCreate)
			where T : IFacility, new()
		{
			return AddFacility(onCreate);
		}
	}
}