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
#if SILVERLIGHT
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
		private readonly List<Burden> instances = new List<Burden>();
		private readonly object @lock  = new object();

		public override void Dispose()
		{
			if (instances.Count == 0)
			{
				return;
			}
			Burden[] array;
			lock(@lock)
			{
				array = instances.ToArray();
				instances.Clear();
			}
			foreach (var burden in array)
			{
				burden.Release();
			}
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			Burden cachedBurden;

			var dictionary = Map;
			if (dictionary.TryGetValue(ComponentActivator, out cachedBurden))
			{
				return cachedBurden.Instance;
			}
			var burden = CreateInstance(context, true);
			dictionary.Add(ComponentActivator, burden);
			lock(@lock)
			{
				instances.Add(burden);
			}
			Track(burden, releasePolicy);
			return burden.Instance;
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