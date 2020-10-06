// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	 http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Windsor.Extensions.DependencyInjection.Extensions
{
	using System;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Registration.Lifestyle;

	using Castle.Windsor.Extensions.DependencyInjection.Scope;

	public static class WindsorExtensions
	{
		/// <summary>
		/// Scopes the lifestyle of the component to a scope started by <see name="IServiceScopeFactory.CreateScope" />
		/// </summary>
		/// <typeparam name="TService">Service type</typeparam>
		public static ComponentRegistration<TService> ScopedToNetServiceScope<TService>(this LifestyleGroup<TService> lifestyle) where TService : class
		{
			return lifestyle.Scoped<ExtensionContainerScopeAccessor>();
		}

		/// <summary>
		/// Returns new instances everytime it's resolved but disposes it on <see name="IServiceScope" /> end
		/// </summary>
		/// <typeparam name="TService">Service type</typeparam>
		public static ComponentRegistration<TService> LifestyleNetTransient<TService>(this ComponentRegistration<TService> registration) where TService : class
		{
			return registration
				.Attribute(ExtensionContainerScope.TransientMarker).Eq(Boolean.TrueString)
				.LifeStyle.ScopedToNetServiceScope();  //.NET core expects new instances but release on scope dispose
		}

		/// <summary>
		/// Singleton instance with .NET Core semantics
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		public static ComponentRegistration<TService> NetStatic<TService>(this LifestyleGroup<TService> lifestyle) where TService : class
		{
			return lifestyle
				.Scoped<ExtensionContainerRootScopeAccessor>();
		}

		/// <summary>
		/// Scopes the lifestyle of the component to a scope started by <see name="IServiceScopeFactory.CreateScope" />
		/// </summary>
		/// <param name="descriptor">Service descriptor</param>
		/// <returns></returns>
		public static BasedOnDescriptor ScopedToNetServiceScope(this BasedOnDescriptor descriptor)
		{
			return descriptor.Configure(reg => reg.LifeStyle.ScopedToNetServiceScope());
		}

		/// <summary>
		/// Returns new instances everytime it's resolved but disposes it on <see name="IServiceScope" /> end
		/// </summary>
		/// <param name="descriptor">Service descriptor</param>
		/// <returns></returns>
		public static BasedOnDescriptor LifestyleNetTransient(this BasedOnDescriptor descriptor)
		{
			return descriptor.Configure(reg => reg.LifestyleNetTransient());
		}

		/// <summary>
		/// Singleton instance with .NET Core semantics
		/// </summary>
		/// <param name="descriptor">Service descriptor</param>
		/// <returns></returns>
		public static BasedOnDescriptor LifestyleNetStatic(this BasedOnDescriptor descriptor)
		{
			return descriptor.Configure(reg => reg.LifeStyle.NetStatic());
		}
	}
}