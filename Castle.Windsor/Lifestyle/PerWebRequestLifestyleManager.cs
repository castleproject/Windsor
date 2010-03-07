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

#if (!SILVERLIGHT)
namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Collections.Generic;
	using System.Web;

	/// <summary>
	/// Implements a Lifestyle Manager for Web Apps that
	/// create at most one object per web request.
	/// </summary>
	[Serializable]
	public class PerWebRequestLifestyleManager : AbstractLifestyleManager
	{

		private string PerRequestObjectID = "PerRequestLifestyleManager_" + Guid.NewGuid();
		private bool evicting;

		#region ILifestyleManager Members

		public override object Resolve(CreationContext context)
		{
			HttpContext current = HttpContext.Current;

			if (current == null)
				throw new InvalidOperationException(
					"HttpContext.Current is null. PerWebRequestLifestyle can only be used in ASP.Net");

			if (current.Items[PerRequestObjectID] == null)
			{
				if (!PerWebRequestLifestyleModule.Initialized)
				{
					string message = string.Format("Looks like you forgot to register the http module {0}{1}Add '<add name=\"PerRequestLifestyle\" type=\"Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule, Castle.Windsor\" />' to the <httpModules> section on your web.config", typeof(PerWebRequestLifestyleModule).FullName, Environment.NewLine);

					throw new Exception(message);
				}

				object instance = base.Resolve(context);
				current.Items[PerRequestObjectID] = instance;
				PerWebRequestLifestyleModule.RegisterForEviction(this, instance);
			}

			return current.Items[PerRequestObjectID];
		}

		public override bool Release(object instance)
		{
			// Since this method is called by the kernel when an external
			// request to release the component is made, it must do nothing
			// to ensure the component is available during the duration of 
			// the web request.  An internal Evict method is provided to
			// allow the actual releasing of the component at the end of
			// the web request.

			if (evicting == false) return false;

			return base.Release(instance);
		}

		internal void Evict(object instance)
		{
			using (new EvitionScope(this))
			{
				Kernel.ReleaseComponent(instance);
			}
		}

		public override void Dispose()
		{
		}

		#endregion

		private class EvitionScope : IDisposable
		{
			private readonly PerWebRequestLifestyleManager owner;

			public EvitionScope(PerWebRequestLifestyleManager owner)
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

	public class PerWebRequestLifestyleModule : IHttpModule
	{
		private static bool initialized;

		private const string PerRequestEvict = "PerRequestLifestyleManager_Evict";

		public void Init(HttpApplication context)
		{
			initialized = true;
			context.EndRequest += Application_EndRequest;
		}

		public void Dispose()
		{
		}

		internal static void RegisterForEviction(PerWebRequestLifestyleManager manager, object instance)
		{
			HttpContext context = HttpContext.Current;

			var candidates = (IDictionary<PerWebRequestLifestyleManager, object>)context.Items[PerRequestEvict];

			if (candidates == null)
			{
				candidates = new Dictionary<PerWebRequestLifestyleManager, object>();
				context.Items[PerRequestEvict] = candidates;
			}

			candidates.Add(manager, instance);
		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{
			var application = (HttpApplication)sender;
			var candidates = (IDictionary<PerWebRequestLifestyleManager, object>)application.Context.Items[PerRequestEvict];

			if (candidates != null)
			{
				foreach (var candidate in candidates)
				{
					var manager = candidate.Key;
					manager.Evict(candidate.Value);
				}

				application.Context.Items.Remove(PerRequestEvict);
			}
		}

		internal static bool Initialized
		{
			get { return initialized; }
		}
	}

	#endregion
}
#endif
