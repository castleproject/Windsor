// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore
{
	using System;
	using System.Reflection;

	using Castle.Facilities.AspNetCore.Activators;

	using Microsoft.AspNetCore.Mvc.Controllers;
	using Microsoft.AspNetCore.Mvc.Internal;
	using Microsoft.AspNetCore.Mvc.Razor;
	using Microsoft.AspNetCore.Mvc.Razor.Internal;
	using Microsoft.AspNetCore.Mvc.ViewComponents;
	using Microsoft.Extensions.DependencyInjection;

	internal static class AspNetCoreMvcExtensions
	{
		public static void AddCustomControllerActivation(this IServiceCollection services, Func<Type, object> activator)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (activator == null) throw new ArgumentNullException(nameof(activator));

			services.AddSingleton<IControllerActivator>(new DelegatingControllerActivator(context => activator(context.ActionDescriptor.ControllerTypeInfo.AsType())));
		}

		public static void AddCustomViewComponentActivation(this IServiceCollection services, Func<Type, object> activator)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (activator == null) throw new ArgumentNullException(nameof(activator));

			services.AddSingleton<IViewComponentActivator>(new DelegatingViewComponentActivator(activator));
		}

		public static void AddCustomTagHelperActivation(this IServiceCollection services, Func<Type, object> activator, Predicate<Type> applicationTypeSelector = null)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (activator == null) throw new ArgumentNullException(nameof(activator));

			applicationTypeSelector = applicationTypeSelector ?? (type => !type.GetTypeInfo().Namespace.StartsWith("Microsoft"));

			services.AddSingleton<ITagHelperActivator>(provider =>
				new DelegatingTagHelperActivator(
					applicationTypeSelector,
					activator,
					new DefaultTagHelperActivator(provider.GetRequiredService<ITypeActivatorCache>())));
		}
	}
}