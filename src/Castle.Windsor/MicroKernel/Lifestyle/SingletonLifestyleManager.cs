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
	using System;

	using Castle.MicroKernel.Context;

	/// <summary>
	///   Only one instance is created first time an instance of the component is requested, and it is then reused for all subseque.
	/// </summary>
	[Serializable]
	public class SingletonLifestyleManager : AbstractLifestyleManager
	{
		private Burden cachedBurden;

		public override void Dispose()
		{
			var localInstance = cachedBurden;
			if (localInstance != null)
			{
				localInstance.Release();
				cachedBurden = null;
			}
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			// 1. read from cache
			if (cachedBurden != null)
			{
				return cachedBurden.Instance;
			}
			var instanceFromContext = context.GetContextualProperty(ComponentActivator);
			if (instanceFromContext != null)
			{
				//we've been called recursively, by some dependency from base.Resolve call
				return instanceFromContext;
			}

			lock (ComponentActivator)
			{
				if (cachedBurden != null)
				{
					return cachedBurden.Instance;
				}
				return GetNewInstance(context, releasePolicy);
			}
		}

		private object GetNewInstance(CreationContext context, IReleasePolicy releasePolicy)
		{
			var burden = CreateInstance(context, true);
			cachedBurden = burden;
			Track(burden, releasePolicy);
			return burden.Instance;
		}
	}
}