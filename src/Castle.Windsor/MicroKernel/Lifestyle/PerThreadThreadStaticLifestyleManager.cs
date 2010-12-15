// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Lifestyle
{
#if (SILVERLIGHT)
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel.Context;

	/// <summary>
	///   per thread LifestyleManager implementation compatible with Silverlight.
	/// </summary>
	public class PerThreadThreadStaticLifestyleManager : AbstractLifestyleManager
	{
		[ThreadStatic]
		private static Dictionary<IComponentActivator, Burden> map;

		/// <summary>
		/// </summary>
		public override void Dispose()
		{
			if (map == null)
			{
				return;
			}

			var dictionary = Map;
			Burden burden;
			if (dictionary.TryGetValue(ComponentActivator, out burden))
			{
				map.Remove(ComponentActivator);
				burden.Release();
			}
		}

		public override object Resolve(CreationContext context, Burden burden, IReleasePolicy releasePolicy)
		{
			Burden cachedBurden;

			var dictionary = Map;
			if (dictionary.TryGetValue(ComponentActivator, out cachedBurden))
			{
				return burden.Instance;
			}
			var instance = base.Resolve(context, burden, releasePolicy);
			dictionary.Add(ComponentActivator, burden);

			return instance;
		}

		protected override void Track(Burden burden, IReleasePolicy releasePolicy)
		{
			var track = burden.RequiresPolicyRelease;
			burden.RequiresPolicyRelease = false;
			if (track)
			{
				releasePolicy.Track(burden.Instance, burden);
			}
		}

		public static Dictionary<IComponentActivator, Burden> Map
		{
			get
			{
				if (map == null)
				{
					map = new Dictionary<IComponentActivator, Burden>();
				}
				return map;
			}
		}
	}
#endif
}