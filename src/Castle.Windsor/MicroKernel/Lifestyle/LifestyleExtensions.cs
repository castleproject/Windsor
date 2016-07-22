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

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.ComponentModel;

	using Castle.Windsor;

#if SILVERLIGHT
	using Scope = Castle.MicroKernel.Lifestyle.Scoped.ThreadStaticLifetimeScope;
#else
	using Scope = Castle.MicroKernel.Lifestyle.Scoped.CallContextLifetimeScope;
#endif

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class LifestyleExtensions
	{
		public static IDisposable BeginScope(this IKernel kernel)
		{
			return new Scope(kernel);
		}

		public static IDisposable RequireScope(this IKernel kernel)
		{
			var current = Scope.ObtainCurrentScope();
			if (current == null)
			{
				return kernel.BeginScope();
			}
			return null;
		}

		public static IDisposable BeginScope(this IWindsorContainer container)
		{
			return new Scope(container);
		}

		public static IDisposable RequireScope(this IWindsorContainer container)
		{
			var current = Scope.ObtainCurrentScope();
			if (current == null)
			{
				return container.BeginScope();
			}
			return null;
		}
	}
}