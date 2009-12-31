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
	using System.Collections.Generic;
	using System.ServiceModel;

	public abstract class AbstractWcfLifestyleCache<TContext> : IWcfLifestyleCache<TContext>
		where TContext : class, IExtensibleObject<TContext>
	{
		private readonly IDictionary<IWcfLifestyle, object> components = new Dictionary<IWcfLifestyle, object>(new WcfLifestyleComparer());
		private TContext channel;
		private bool used;

		public object this[IWcfLifestyle manager]
		{
			get
			{
				object component;
				components.TryGetValue(manager, out component);
				return component;
			}
			set
			{
				components[manager] = value;
			}
		}

		public void Attach(TContext owner)
		{
			if (used)
			{
				throw new InvalidOperationException("This instance was already used!");
			}

			if (channel != null)
			{
				throw new InvalidOperationException("Can't attach twice!");
			}

			used = true;
			channel = owner;
			InitContext(channel);
		}

		public void Detach(TContext owner)
		{
			if (!used)
			{
				throw new InvalidOperationException("This instance was not used!");
			}

			if (channel == null)
			{
				throw new InvalidOperationException("Can't Detach twice or before attaching!");
			}

			ShutdownContext(channel);
			channel = null;
		}

		protected void ShutdownCache()
		{
			channel.Extensions.Remove(this);
			foreach (var component in components)
			{
				component.Key.Release(component.Value);
			}

			components.Clear();
		}

		protected abstract void ShutdownContext(TContext context);

		protected abstract void InitContext(TContext context);

		private class WcfLifestyleComparer : IEqualityComparer<IWcfLifestyle>
		{
			public bool Equals(IWcfLifestyle x, IWcfLifestyle y)
			{
				return x.ComponentId.Equals(y.ComponentId);
			}

			public int GetHashCode(IWcfLifestyle obj)
			{
				return obj.ComponentId.GetHashCode();
			}
		}
	}
}