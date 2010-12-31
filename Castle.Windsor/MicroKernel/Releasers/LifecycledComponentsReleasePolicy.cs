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
	using Castle.MicroKernel.Util;

	/// <summary>
	///   Tracks all components if asked. Releases those requiring decomission (<see cref = "Burden.RequiresPolicyRelease" />)
	/// </summary>
	[Serializable]
	public class LifecycledComponentsReleasePolicy : IReleasePolicy
	{
		private readonly Dictionary<object, Burden> instance2Burden =
			new Dictionary<object, Burden>(ReferenceEqualityComparer.Instance);

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
					if (burden.Value.RequiresPolicyRelease)
					{
						burden.Value.Release();
					}
				}
			}
		}

		public IReleasePolicy CreateSubPolicy()
		{
			return new SubReleasePolicy(this);
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

			using (@lock.ForWriting())
			{
				Burden burden;
				if (!instance2Burden.TryGetValue(instance, out burden))
				{
					return;
				}
				if (burden.RequiresPolicyRelease == false)
				{
					return;
				}
				burden.Release();
			}
		}

		public virtual void Track(object instance, Burden burden)
		{
			using (@lock.ForWriting())
			{
				instance2Burden.Add(instance, burden);
				burden.Released += OnInstanceReleased;
			}
		}

		private void OnInstanceReleased(Burden burden)
		{
			using (@lock.ForWriting())
			{
				if (instance2Burden.Remove(burden.Instance))
				{
					burden.Released -= OnInstanceReleased;
				}
			}
		}
	}
}