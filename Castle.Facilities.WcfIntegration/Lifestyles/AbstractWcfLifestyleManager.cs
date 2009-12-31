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

namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;
	using System.ServiceModel;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle;

	public abstract class AbstractWcfLifestyleManager<TExtensibleObject, TCache> : AbstractLifestyleManager, IWcfLifestyle
		where TExtensibleObject : class, IExtensibleObject<TExtensibleObject>
		where TCache : class, IWcfLifestyleCache<TExtensibleObject>, new()
	{

		private readonly Guid id = Guid.NewGuid();

		public Guid ComponentId
		{
			get { return id; }
		}

		public override void Dispose()
		{

		}

		public override object Resolve(CreationContext context)
		{
			var cacheHolder = GetCacheHolder();
			if (cacheHolder == null)
			{
				return base.Resolve(context);
			}

			var cache = cacheHolder.Extensions.Find<TCache>();
			if (cache == null)
			{
				cache = new TCache();
				cacheHolder.Extensions.Add(cache);
			}

			var component = cache[this];
			if (component == null)
			{
				component = base.Resolve(context);
				cache[this] = component;
			}

			return component;
		}

		protected abstract TExtensibleObject GetCacheHolder();
	}
}