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


#if FEATURE_SYSTEM_WEB

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.ComponentModel;
	using System.Reflection;
	using System.Security;
    using System.Web;

	[SecurityCritical]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class PerWebRequestLifestyleModuleRegistration
	{
		internal const string MicrosoftWebInfrastructureDll =
			"Microsoft.Web.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

		public static void Run()
		{
			HttpApplication.RegisterModule(typeof(PerWebRequestLifestyleModule));
		}
	}
}

#endif
