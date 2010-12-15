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

#if !(SILVERLIGHT || CLIENTPROFILE)
namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public class PerWebRequestLifestyleModule : IHttpModule
	{
		private const string PerRequestEvict = "PerRequestLifestyleManager_Evict";
		private static bool initialized;

		public void Dispose()
		{
		}

		public void Init(HttpApplication context)
		{
			initialized = true;
			context.EndRequest += Application_EndRequest;
		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{
			var application = (HttpApplication)sender;
			var candidates = (IDictionary<PerWebRequestLifestyleManager, Burden>)application.Context.Items[PerRequestEvict];

			if (candidates == null)
			{
				return;
			}

			// NOTE: This is relying on a undocumented behavior that order of items when enumeratinc Dictionary<> will be oldest --> latest
			foreach (var candidate in candidates.Reverse())
			{
				candidate.Value.Release();
			}

			application.Context.Items.Remove(PerRequestEvict);
		}

		internal static bool Initialized
		{
			get { return initialized; }
		}

		internal static void RegisterForEviction(PerWebRequestLifestyleManager manager, Burden burden)
		{
			var context = HttpContext.Current;

			var candidates = (IDictionary<PerWebRequestLifestyleManager, Burden>)context.Items[PerRequestEvict];

			if (candidates == null)
			{
				candidates = new Dictionary<PerWebRequestLifestyleManager, Burden>();
				context.Items[PerRequestEvict] = candidates;
			}

			candidates.Add(manager, burden);
		}
	}
}
#endif