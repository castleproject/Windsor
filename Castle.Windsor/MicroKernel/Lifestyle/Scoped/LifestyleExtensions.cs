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

namespace Castle.MicroKernel.Lifestyle.Scoped
{
	using System;

	using Castle.Windsor;

	public static class LifestyleExtensions
	{
		public static IDisposable BeginScope(this IKernel kernel)
		{
			return GetScopeSubSystem(kernel).BeginScope();
		}

		public static IDisposable BeginScope(this IWindsorContainer container)
		{
			return BeginScope(container.Kernel);
		}

		public static IDisposable RequireScope(this IKernel kernel)
		{
			return GetScopeSubSystem(kernel).RequireScope();
		}

		public static IDisposable RequireScope(this IWindsorContainer container)
		{
			return RequireScope(container.Kernel);
		}

		private static IScopeManager GetScopeSubSystem(IKernel kernel)
		{
			var scopes = (IScopeManager)kernel.GetSubSystem("scope");
			if (scopes == null)
			{
				throw new InvalidCastException("Unable to obtain the ScopeSubsystem.  " +
				                               "Did you forget to add it using the key 'scope'?");
			}
			return scopes;
		}
	}
}