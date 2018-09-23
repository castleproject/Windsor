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
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.Registration;

	/// <summary>
	/// For overriding default registration and lifestyles behaviour
	/// </summary>
	public class WindsorRegistrationOptions
	{
		private Assembly entryAssembly = null;

		internal Assembly EntryAssembly
		{
			get
			{
				try
				{
					entryAssembly = entryAssembly ?? Assembly.GetEntryAssembly();
				}
				catch
				{
					entryAssembly = entryAssembly ?? Assembly.GetCallingAssembly();
				}

				return entryAssembly;
			}
		}

		internal List<(Assembly, LifestyleType)> ControllerAssemblyRegistrations = new List<(Assembly, LifestyleType)>();
		internal List<(Assembly, LifestyleType)> TagHelperAssemblyRegistrations = new List<(Assembly, LifestyleType)>();
		internal List<(Assembly, LifestyleType)> ViewComponentAssemblyRegistrations = new List<(Assembly, LifestyleType)>();

		internal List<IRegistration> ControllerComponentRegistrations = new List<IRegistration>();
		internal List<IRegistration> TagHelperComponentRegistrations = new List<IRegistration>();
		internal List<IRegistration> ViewComponentComponentRegistrations = new List<IRegistration>();

		/// <summary>
		/// Use this method to specify where controllers, tagHelpers and viewComponents are registered from. Use this method
		/// if the facility starts throwing ComponentNotFoundExceptions because of problems with <see cref="Assembly.GetCallingAssembly"/>/<see cref="Assembly.GetEntryAssembly"/>.
		/// You can optionally use <see cref="RegisterControllers(Assembly, LifestyleType)"/>/<see cref="RegisterTagHelpers(Assembly, LifestyleType)"/>/<see cref="RegisterViewComponents(Assembly, LifestyleType)"/> if you need more fine grained
		/// control for sourcing these framework components.
		/// </summary>
		/// <param name="entryAssembly"></param>
		/// <returns><see cref="WindsorRegistrationOptions"/></returns>
		public WindsorRegistrationOptions UseEntryAssembly(Assembly entryAssembly)
		{
			this.entryAssembly = entryAssembly;
			return this;
		}

		/// <summary>
		/// Use this method to customise the registration/lifestyle of controllers.
		/// </summary>
		/// <param name="controllersAssembly">Assembly where the controllers are defined. Defaults to <see cref="Assembly.GetCallingAssembly"/>.</param>
		/// <param name="lifestyleType">The lifestyle of the controllers. Defaults to <see cref="LifestyleType.Scoped"/>.</param>
		/// <returns><see cref="WindsorRegistrationOptions"/> as a fluent interface</returns>
		public WindsorRegistrationOptions RegisterControllers(Assembly controllersAssembly = null, LifestyleType lifestyleType = LifestyleType.Scoped)
		{
			ControllerAssemblyRegistrations.Add((controllersAssembly ?? EntryAssembly, lifestyleType));
			return this;
		}

		/// <summary>
		/// Use this method for customising the registration of controllers.
		/// </summary>
		/// <param name="registrations"><see cref="ComponentRegistration"/> for more details</param>
		/// <returns></returns>
		/// <returns><see cref="WindsorRegistrationOptions"/> as a fluent interface</returns>
		public WindsorRegistrationOptions RegisterControllers(params IRegistration[] registrations)
		{
			ControllerComponentRegistrations.AddRange(registrations);
			return this;
		}

		/// <summary>
		/// Use this method to customise the registration/lifestyle of tagHelpers.
		/// </summary>
		/// <param name="tagHelpersAssembly">Assembly where the tag helpers are defined. Defaults to Assembly.GetCallingAssembly().</param>
		/// <param name="lifestyleType">The lifestyle of the controllers. Defaults to <see cref="LifestyleType.Scoped"/>.</param>
		/// <returns><see cref="WindsorRegistrationOptions"/> as a fluent interface</returns>
		public WindsorRegistrationOptions RegisterTagHelpers(Assembly tagHelpersAssembly = null, LifestyleType lifestyleType = LifestyleType.Scoped)
		{
			TagHelperAssemblyRegistrations.Add((tagHelpersAssembly ?? EntryAssembly, lifestyleType));
			return this;
		}

		/// <summary>
		/// Use this method for customising the registration of TagHelpers.
		/// </summary>
		/// <param name="registrations"><see cref="ComponentRegistration"/> for more details</param>
		/// <returns></returns>
		/// <returns><see cref="WindsorRegistrationOptions"/> as a fluent interface</returns>
		public WindsorRegistrationOptions RegisterTagHelpers(params IRegistration[] registrations)
		{
			TagHelperComponentRegistrations.AddRange(registrations);
			return this;
		}

		/// <summary>
		/// Use this method to customise the registration/lifestyle of view components.
		/// </summary>
		/// <param name="viewComponentsAssembly">Assembly where the view components are defined. Defaults to Assembly.GetCallingAssembly().</param>
		/// <param name="lifestyleType">The lifestyle of the controllers. Defaults to <see cref="LifestyleType.Scoped"/>.</param>
		/// <returns><see cref="WindsorRegistrationOptions"/> as a fluent interface</returns>
		public WindsorRegistrationOptions RegisterViewComponents(Assembly viewComponentsAssembly = null, LifestyleType lifestyleType = LifestyleType.Scoped)
		{
			ViewComponentAssemblyRegistrations.Add((viewComponentsAssembly ?? EntryAssembly, lifestyleType));
			return this;
		}

		/// <summary>
		/// Use this method for customising the registration of ViewComponents.
		/// </summary>
		/// <param name="registrations"><see cref="ComponentRegistration"/> for more details</param>
		/// <returns></returns>
		/// <returns><see cref="WindsorRegistrationOptions"/> as a fluent interface</returns>
		public WindsorRegistrationOptions RegisterViewComponents(params IRegistration[] registrations)
		{
			ViewComponentComponentRegistrations.AddRange(registrations);
			return this;
		}
	}
}