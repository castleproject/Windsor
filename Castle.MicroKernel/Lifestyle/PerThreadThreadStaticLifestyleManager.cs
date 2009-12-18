// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// per thread LifestyleManager implementation compatibile with Silverlight.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class PerThreadThreadStaticLifestyleManager : AbstractLifestyleManager
	{
#if (!SILVERLIGHT)
		[NonSerialized]
#endif
		[ThreadStatic]
		private static Dictionary<IComponentActivator, object> map;

		public static Dictionary<IComponentActivator, object> Map
		{
			get
			{
				if (map == null)
				{
					map = new Dictionary<IComponentActivator, object>();
				}
				return map;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Dispose()
		{
			foreach (var instance in Map.Values)
			{
				base.Release(instance);
			}
		}

		public override object Resolve(CreationContext context)
		{
			Object instance;

			var dictionary = Map;
			if (!dictionary.TryGetValue(ComponentActivator, out instance))
			{
				instance = base.Resolve(context);
				dictionary.Add(ComponentActivator, instance);
			}

			return instance;
		}

		public override bool Release(object instance)
		{
			// Do nothing.
			return false;
		}

	}
}
