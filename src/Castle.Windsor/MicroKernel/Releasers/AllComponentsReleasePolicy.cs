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

namespace Castle.MicroKernel.Releasers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core.Internal;

#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class AllComponentsReleasePolicy : IReleasePolicy
	{
		private readonly IDictionary<object, Burden> instance2Burden =
			new Dictionary<object, Burden>(new Util.ReferenceEqualityComparer());

		private readonly Lock @lock = Lock.Create();

		public virtual void Track(object instance, Burden burden)
		{
			using(@lock.ForWriting())
			{
				instance2Burden[instance] = burden;
			}
		}

		public bool HasTrack(object instance)
		{
			if (instance == null) throw new ArgumentNullException("instance");

			using(@lock.ForReading())
			{
				return instance2Burden.ContainsKey(instance);
			}
		}

		public void Release(object instance)
		{
			if (instance == null) throw new ArgumentNullException("instance");

			using (var locker = @lock.ForReadingUpgradeable())
			{
				Burden burden;

				if (!instance2Burden.TryGetValue(instance, out burden))
					return;

				locker.Upgrade();
				if (!instance2Burden.TryGetValue(instance, out burden))
					return;

				if (instance2Burden.Remove(instance))
				{
					if (burden.Release(this) == false)
					{
						instance2Burden[instance] = burden;
					}
				}
			}
		}

		public void Dispose()
		{
			using(@lock.ForWriting())
			{
				var burdens = new KeyValuePair<object, Burden>[instance2Burden.Count];
				instance2Burden.CopyTo(burdens, 0);

				// NOTE: This is relying on a undocumented behavior that order of items when enumerating Dictionary<> will be oldest --> latest
				foreach (var burden in burdens.Reverse())
				{
					if (instance2Burden.ContainsKey(burden.Key))
					{
						if (burden.Value.Release(this))
						{
							instance2Burden.Remove(burden.Key);
						}
					}
				}
			}
		}
	}
}
