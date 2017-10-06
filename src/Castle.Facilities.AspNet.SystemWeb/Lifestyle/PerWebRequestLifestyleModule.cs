// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNet.SystemWeb.Lifestyle
{
	using System;
	using System.Text;
	using System.Web;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle.Scoped;

	public class PerWebRequestLifestyleModule : IHttpModule
	{
		private static bool initialized;
		private const string Key = "castle.per-web-request-lifestyle-cache";

		public void Init(HttpApplication context)
		{
			initialized = true;
			context.EndRequest += EndRequest;
		}

		protected void EndRequest(Object sender, EventArgs e)
		{
			var scope = GetScope(HttpContext.Current, createIfNotPresent: false);
			scope?.Dispose();
		}

		internal static ILifetimeScope GetScope()
		{
			EnsureInitialized();

			var context = HttpContext.Current;
			if (context == null)
			{
				throw new InvalidOperationException("HttpContext.Current is null. PerWebRequestLifestyle can only be used in ASP.Net");
			}

			return GetScope(context, createIfNotPresent: true);
		}

		internal static ILifetimeScope YieldScope()
		{
			var context = HttpContext.Current;
			if (context == null)
			{
				return null;
			}

			var scope = GetScope(context, createIfNotPresent: true);
			if (scope != null)
			{
				context.Items.Remove(Key);
			}

			return scope;
		}

		private static void EnsureInitialized()
		{
			if (initialized)
			{
				return;
			}

			var message = new StringBuilder();

			message.AppendLine($"Looks like you forgot to register the http module {typeof(PerWebRequestLifestyleModule).FullName}");
			message.AppendLine("To fix this add");
			message.AppendLine("<add name=\"PerRequestLifestyle\" type=\"Castle.Facilities.SystemWeb.Lifestyle.PerWebRequestLifestyleModule, Castle.SystemWebFacility\" />");
			message.AppendLine("to the <httpModules> section on your web.config.");

			if (HttpRuntime.UsingIntegratedPipeline)
			{
				message.AppendLine("Windsor also detected you're running IIS in Integrated Pipeline mode. This means that you also need to add the module to the <modules> section under <system.webServer>.");
			}
			else
			{
				message.AppendLine("If you plan running on IIS in Integrated Pipeline mode, you also need to add the module to the <modules> section under <system.webServer>.");
			}

			message.AppendLine($"Alternatively make sure you have {PerWebRequestLifestyleModuleRegistration.MicrosoftWebInfrastructureDll} assembly in your GAC (it is installed by ASP.NET MVC3 or WebMatrix) and Windsor will be able to register the module automatically without having to add anything to the config file.");

			throw new ComponentResolutionException(message.ToString());
		}

		private static ILifetimeScope GetScope(HttpContext context, bool createIfNotPresent)
		{
			var candidates = (ILifetimeScope)context.Items[Key];
			if (candidates == null && createIfNotPresent)
			{
				candidates = new DefaultLifetimeScope(new ScopeCache());
				context.Items[Key] = candidates;
			}
			return candidates;
		}

		public void Dispose()
		{
		}
	}
}
