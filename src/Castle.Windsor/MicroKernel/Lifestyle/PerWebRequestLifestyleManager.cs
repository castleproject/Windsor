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
	using System.Web;

	using Castle.MicroKernel.Context;

	/// <summary>
	///   Implements a Lifestyle Manager for Web Apps that
	///   create at most one object per web request.
	/// </summary>
	[Serializable]
	public class PerWebRequestLifestyleManager : AbstractLifestyleManager
	{
		private readonly string PerRequestObjectID = "PerRequestLifestyleManager_" + Guid.NewGuid();
		private bool evicting;

		public override void Dispose()
		{
			var current = HttpContext.Current;
			if (current == null)
			{
				return;
			}

			var instance = current.Items[PerRequestObjectID];
			if (instance == null)
			{
				return;
			}

			Evict(instance);
		}

		public override bool Release(object instance)
		{
			// Since this method is called by the kernel when an external
			// request to release the component is made, it must do nothing
			// to ensure the component is available during the duration of 
			// the web request.  An internal Evict method is provided to
			// allow the actual releasing of the component at the end of
			// the web request.

			if (evicting == false)
			{
				return false;
			}

			return base.Release(instance);
		}

		public override object Resolve(CreationContext context)
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				throw new InvalidOperationException(
					"HttpContext.Current is null. PerWebRequestLifestyle can only be used in ASP.Net");
			}

			var instance = current.Items[PerRequestObjectID];
			if (instance != null)
			{
				return instance;
			}
			if (!PerWebRequestLifestyleModule.Initialized)
			{
				var message =
					string.Format(
						"Looks like you forgot to register the http module {0}{1}Add '<add name=\"PerRequestLifestyle\" type=\"Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule, Castle.Windsor\" />' to the <httpModules> section on your web.config. If you're running IIS7 in Integrated Mode you will need to add it to <modules> section under <system.webServer>",
						typeof(PerWebRequestLifestyleModule).FullName, Environment.NewLine);

				throw new Exception(message);
			}

			instance = base.Resolve(context);
			current.Items[PerRequestObjectID] = instance;
			PerWebRequestLifestyleModule.RegisterForEviction(this, instance);

			return instance;
		}

		public override void Track(Burden burden, IReleasePolicy releasePolicy)
		{
			return;
		}

		internal void Evict(object instance)
		{
			using (new EvictionScope(this))
			{
				// that's not really thread safe, should we care about it?
				Kernel.ReleaseComponent(instance);
			}
		}

		private class EvictionScope : IDisposable
		{
			private readonly PerWebRequestLifestyleManager owner;

			public EvictionScope(PerWebRequestLifestyleManager owner)
			{
				this.owner = owner;
				this.owner.evicting = true;
			}

			public void Dispose()
			{
				owner.evicting = false;
			}
		}
	}

	#region PerWebRequestLifestyleModule

	#endregion
}

#endif