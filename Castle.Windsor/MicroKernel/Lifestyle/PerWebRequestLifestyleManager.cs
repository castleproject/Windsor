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

#if !SILVERLIGHT
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
		private readonly string perRequestObjectId = "PerRequestLifestyleManager_" + Guid.NewGuid();

		public override void Dispose()
		{
			var current = HttpContext.Current;
			if (current == null)
			{
				return;
			}

			var burden = (Burden)current.Items[perRequestObjectId];
			if (burden == null)
			{
				return;
			}
			burden.Release();
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				throw new InvalidOperationException(
					"HttpContext.Current is null. PerWebRequestLifestyle can only be used in ASP.Net");
			}

			var cachedBurden = (Burden)current.Items[perRequestObjectId];
			if (cachedBurden != null)
			{
				return cachedBurden.Instance;
			}
			if (!PerWebRequestLifestyleModule.Initialized)
			{
				var message =
					string.Format(
						"Looks like you forgot to register the http module {0}{1}Add '<add name=\"PerRequestLifestyle\" type=\"Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule, Castle.Windsor\" />' to the <httpModules> section on your web.config. If you're running IIS7 in Integrated Mode you will need to add it to <modules> section under <system.webServer>",
						typeof(PerWebRequestLifestyleModule).FullName, Environment.NewLine);

				throw new Exception(message);
			}

			var burden = base.CreateInstance(context, true);
			current.Items[perRequestObjectId] = burden;
			PerWebRequestLifestyleModule.RegisterForEviction(this, burden);
			Track(burden, releasePolicy);
			return burden.Instance;
		}
	}
}
#endif