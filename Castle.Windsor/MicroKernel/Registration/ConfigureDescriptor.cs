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

namespace Castle.MicroKernel.Registration
{
	using System;

	/// <summary>
	///   Describes a configuration.
	/// </summary>
	public class ConfigureDescriptor
	{
		private readonly Type baseType;
		private readonly Action<ComponentRegistration> configurer;

		/// <summary>
		///   Initializes a new instance of the ConfigureDescriptor.
		/// </summary>
		/// <param name = "configurer">The configuration action.</param>
		public ConfigureDescriptor(Action<ComponentRegistration> configurer)
			: this(null, configurer)
		{
		}

		///<summary>
		///  Initializes a new instance of the ConfigureDescriptor.
		///</summary>
		///<param name = "baseType">The base type to match.</param>
		///<param name = "configurer">The configuration action.</param>
		public ConfigureDescriptor(Type baseType, Action<ComponentRegistration> configurer)
		{
			this.baseType = baseType;
			this.configurer = configurer;
		}

		/// <summary>
		///   Performs the component configuration.
		/// </summary>
		/// <param name = "registration">The component registration.</param>
		public virtual void Apply(ComponentRegistration registration)
		{
			if (baseType == null || baseType.IsAssignableFrom(registration.Implementation))
			{
				configurer(registration);
			}
		}
	}
}