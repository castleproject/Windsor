#region license

// Copyright 2009-2011 Castle Project, http://castleproject.org and Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Castle.Services.vNextTransaction.NHibernate
{
	public class PerWebRequestLifestyleModule : IHttpModule
	{
		private static bool initialized;
		private const string PerRequestEvict = "PerRequestLifestyleManager_Evict";

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			var application = (HttpApplication) sender;
			var candidates =
				(IDictionary<PerTransaction, object>) application.Context.Items[PerRequestEvict];
			if (candidates == null) return;

			foreach (var candidate in candidates.Reverse())
				candidate.Key.Evict(candidate.Value);

			application.Context.Items.Remove(PerRequestEvict);
		}

		public static bool Initialized
		{
			get { return initialized; }
		}

		public void Init(HttpApplication context)
		{
			initialized = true;
			context.EndRequest += Application_EndRequest;
		}

		internal static void RegisterForEviction(PerTransaction manager, object instance)
		{
			var context = HttpContext.Current;
			var candidates = (IDictionary<PerTransaction, object>) context.Items[PerRequestEvict];

			if (candidates == null)
			{
				candidates = new Dictionary<PerTransaction, object>();
				context.Items[PerRequestEvict] = candidates;
			}

			candidates.Add(manager, instance);
		}

		public void Dispose()
		{
		}

		// Properties
	}
}