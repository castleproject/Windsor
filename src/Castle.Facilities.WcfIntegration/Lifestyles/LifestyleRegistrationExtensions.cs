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

namespace Castle.Facilities.WcfIntegration
{
	using System.ComponentModel;

	using Castle.Facilities.WcfIntegration.Lifestyles;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Registration.Lifestyle;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class LifestyleRegistrationExtensions
	{
		public static BasedOnDescriptor LifestylePerWcfOperation(this BasedOnDescriptor descriptor)
		{
			return descriptor.Configure(c => c.LifestylePerWcfOperation());
		}

		public static ComponentRegistration<TService> LifestylePerWcfOperation<TService>(this ComponentRegistration<TService> registration) where TService : class
		{
			return registration.LifeStyle.Scoped<WcfOperationScopeAccessor>();
		}

		public static BasedOnDescriptor LifestylePerWcfSession(this BasedOnDescriptor descriptor)
		{
			return descriptor.Configure(c => c.LifestylePerWcfSession());
		}

		public static ComponentRegistration<TService> LifestylePerWcfSession<TService>(this ComponentRegistration<TService> registration) where TService : class
		{
			return registration.LifeStyle.Scoped<WcfSessionScopeAccessor>();
		}

		public static ComponentRegistration<TService> PerWcfOperation<TService>(this LifestyleGroup<TService> @group) where TService : class
		{
			return group.Scoped<WcfOperationScopeAccessor>();
		}

		public static ComponentRegistration<TService> PerWcfSession<TService>(this LifestyleGroup<TService> @group) where TService : class
		{
			return group.Scoped<WcfSessionScopeAccessor>();
		}
	}
}