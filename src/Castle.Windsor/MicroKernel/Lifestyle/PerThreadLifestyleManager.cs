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

#if (!SILVERLIGHT)
namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Runtime.Serialization;

	using Castle.MicroKernel.Context;

	/// <summary>
	///   Summary description for PerThreadLifestyleManager.
	/// </summary>
	[Serializable]
	public class PerThreadLifestyleManager : AbstractLifestyleManager, IDeserializationCallback
	{
		[NonSerialized]
		private static LocalDataStoreSlot slot = Thread.AllocateNamedDataSlot("CastlePerThread");

		[NonSerialized]
		private IList<Burden> instances = new List<Burden>();

		public override void Dispose()
		{
			foreach (var instance in instances)
			{
				instance.Release();
			}

			instances.Clear();

			// This doesn't seem right. it will collapse if there are multiple instances of the container
			Thread.FreeNamedDataSlot("CastlePerThread");
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			lock (slot)
			{
				var map = (Dictionary<IComponentActivator, Burden>)Thread.GetData(slot);

				if (map == null)
				{
					map = new Dictionary<IComponentActivator, Burden>();

					Thread.SetData(slot, map);
				}

				Burden burden;
				if (!map.TryGetValue(ComponentActivator, out burden))
				{
					burden = base.CreateInstance(context, true);
					map.Add(ComponentActivator, burden);
					instances.Add(burden);
					Track(burden, releasePolicy);
				}

				return burden.Instance;
			}
		}

		public void OnDeserialization(object sender)
		{
			slot = Thread.AllocateNamedDataSlot("CastlePerThread");
			instances = new List<Burden>();
		}
	}
}

#endif