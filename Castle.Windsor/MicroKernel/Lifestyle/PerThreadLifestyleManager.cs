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
#if (!SILVERLIGHT)
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Runtime.Serialization;

	using Castle.MicroKernel.Context;

	/// <summary>
	/// Summary description for PerThreadLifestyleManager.
	/// </summary>
	[Serializable]
	public class PerThreadLifestyleManager : AbstractLifestyleManager, IDeserializationCallback
	{
		[NonSerialized]
		private static LocalDataStoreSlot slot = Thread.AllocateNamedDataSlot("CastlePerThread");

		[NonSerialized]
		private IList<object> instances =  new List<object>();

		/// <summary>
		/// 
		/// </summary>
		public override void Dispose()
		{
			foreach( object instance in instances )
			{
				base.Release( instance );
			}

			instances.Clear();

			// This doesn't seem right. it will collapse if there are multiple instances of the container
			Thread.FreeNamedDataSlot( "CastlePerThread" );
		}

		public override object Resolve(CreationContext context)
		{
			lock(slot)
			{
				var map = (Dictionary<IComponentActivator, object>)Thread.GetData(slot);

				if (map == null)
				{
					map = new Dictionary<IComponentActivator, object>();

					Thread.SetData( slot, map );
				}

				Object instance;

				if (!map.TryGetValue(ComponentActivator, out instance))
				{
					instance = base.Resolve(context);
					map.Add(ComponentActivator, instance);
					instances.Add(instance);
				}

				return instance;
			}
		}

		public override bool Release(object instance)
		{
			// Do nothing.
			return false;
		}
		
		public void OnDeserialization(object sender)
		{
			slot = Thread.AllocateNamedDataSlot("CastlePerThread");
			instances = new List<object>();
		}
	}
#endif
}
