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


#if !(SILVERLIGHT || CLIENTPROFILE || DOTNET35)

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.ComponentModel;
	using System.Reflection;
	using System.Security;

	[SecurityCritical]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class PerWebRequestLifestyleModuleRegistration
	{
		internal const string MicrosoftWebInfrastructureDll =
			"Microsoft.Web.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

		public static void Run()
		{
			var dynamicModuleUtil = Type.GetType("Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility, " + MicrosoftWebInfrastructureDll,
			                                     throwOnError: false);
			if (dynamicModuleUtil == null)
			{
				return;
			}
			var registerModule = dynamicModuleUtil.GetMethod("RegisterModule", BindingFlags.Static | BindingFlags.Public);
			if (registerModule == null)
			{
				return;
			}
			registerModule.Invoke(null, new object[] { typeof(PerWebRequestLifestyleModule) });
		}
	}
}

#endif