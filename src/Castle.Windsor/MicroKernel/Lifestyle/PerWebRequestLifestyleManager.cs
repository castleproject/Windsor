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

	using Castle.MicroKernel.Context;

	/// <summary>
	///   Implements a Lifestyle Manager for Web Apps that
	///   create at most one object per web request.
	/// </summary>
	[Serializable]
	public class PerWebRequestLifestyleManager : AbstractLifestyleManager
	{
		private readonly string instanceId = "PerRequestLifestyleManager_" + Guid.NewGuid();

		public override void Dispose()
		{
			// NOTE: I don't like it.
			PerWebRequestLifestyleModule.Evict(instanceId);
		}

		public override object Resolve(CreationContext context, Burden burden, IReleasePolicy releasePolicy)
		{
			var retrievedBurden = PerWebRequestLifestyleModule.Retrieve(instanceId);
			if (retrievedBurden != null)
			{
				return retrievedBurden;
			}

			var instance = context.GetContextualProperty(ComponentActivator);
			if (instance != null)
			{
				//we've been called recursively, by some dependency from base.Resolve call
				return instance;
			}
			instance = base.CreateInstance(context, burden);
			PerWebRequestLifestyleModule.Store(instanceId, burden);
			burden.RequiresPolicyRelease = false;
			if (burden.RequiresPolicyRelease)
			{
				releasePolicy.Track(burden.Instance, burden);
			}
			return instance;
		}
	}
}
#endif