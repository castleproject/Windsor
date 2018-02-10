// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

// Unconventional namespace retained for ease of upgrading
namespace Castle.MicroKernel.Registration
{
	using Castle.Facilities.AspNet.SystemWeb;
	using Castle.MicroKernel.Registration.Lifestyle;

	public static class LifestyleRegistrationExtensions
	{
		public static BasedOnDescriptor LifestylePerWebRequest(this BasedOnDescriptor descriptor)
		{
			return descriptor.Configure(c => c.LifestylePerWebRequest());
		}

		public static ComponentRegistration<TService> LifestylePerWebRequest<TService>(this ComponentRegistration<TService> registration) where TService : class
		{
			return registration.LifeStyle.Scoped<WebRequestScopeAccessor>();
		}

		public static ComponentRegistration<TService> PerWebRequest<TService>(this LifestyleGroup<TService> @group) where TService : class
		{
			return group.Scoped<WebRequestScopeAccessor>();
		}
	}
}
