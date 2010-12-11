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

	/// <summary>
	///   Tracks all components if asked.
	/// </summary>
	[Serializable]
	public class LifecycledComponentsReleasePolicy : IReleasePolicy
	{
		private readonly Dictionary<object, Burden> instance2Burden =
			new Dictionary<object, Burden>(new Util.ReferenceEqualityComparer());

		private readonly Lock @lock = Lock.Create();

		public void Dispose()
		{
			using (@lock.ForWriting())
			{
				var burdens = instance2Burden.ToArray();
				instance2Burden.Clear();
				// NOTE: This is relying on a undocumented behavior that order of items when enumerating Dictionary<> will be oldest --> latest
				foreach (var burden in burdens.Reverse())
				{
					if(burden.Value.RequiresDecommission)
					{
						burden.Value.Release();
					}
				}
			}
		}

		public bool HasTrack(object instance)
		{
			if (instance == null)
			{
				return false;
			}

			using (@lock.ForReading())
			{
				return instance2Burden.ContainsKey(instance);
			}
		}

		public void Release(object instance)
		{
			if (instance == null)
			{
				return;
			}

			using (var locker = @lock.ForReadingUpgradeable())
			{
				Burden burden;
				if (!instance2Burden.TryGetValue(instance, out burden))
				{
					return;
				}

				locker.Upgrade();
				if (!instance2Burden.TryGetValue(instance, out burden))
				{
					return;
				}
				if (burden.RequiresDecommission == false)
				{
					return;
				}

				// we remove first, then release so that if we recursively end up here again, the first TryGetValue call breaks the circuit
				var existed = instance2Burden.Remove(instance);
				if (existed == false)
				{
					// NOTE: this should not be humanly possible. We should not even have this code here.
					return;
				}

				if (burden.Release() == false)
				{
					// NOTE: ok we didn't remove this component, so let's put it back to the cache so that we can try again later, perhaps with better luck
					instance2Burden[instance] = burden;
				}
			}
		}

		public virtual void Track(object instance, Burden burden)
		{
			using (@lock.ForWriting())
			{
				var oldCount = instance2Burden.Count;
				instance2Burden[instance] = burden;
				if (oldCount < instance2Burden.Count)
				{
					burden.Released += OnInstanceReleased;
				}
			}
		}

		private void OnInstanceReleased(Burden burden)
		{
			using (@lock.ForWriting())
			{
				instance2Burden.Remove(burden.Instance);
				burden.Released -= OnInstanceReleased;
			}
		}
	}
}