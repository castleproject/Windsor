// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.Windsor;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class LifestyleExtensions
	{
		/// <summary>
		/// Begins a scope.
		/// </summary>
		/// <param name="kernel">The <see cref="IKernel"/>.</param>
		/// <returns>Returns an <see cref="IDisposable"/> to be used in a <code>using</code> block.</returns>
		public static IDisposable BeginScope(this IKernel kernel)
		{
			return BeginScope();
		}

		/// <summary>
		/// Begins a scope.
		/// </summary>
		/// <param name="container">The <see cref="IWindsorContainer"/>.</param>
		/// <returns>Returns an <see cref="IDisposable"/> to be used in a <code>using</code> block.</returns>
		public static IDisposable BeginScope(this IWindsorContainer container)
		{
			return BeginScope();
		}

		/// <summary>
		/// Begins a scope if not executing inside one.
		/// </summary>
		/// <param name="kernel">The <see cref="IKernel"/>.</param>
		/// <returns>Returns an <see cref="IDisposable"/> to be used in a <code>using</code> block.</returns>
		public static IDisposable RequireScope(this IKernel kernel)
		{
			return RequireScope();
		}

		/// <summary>
		/// Begins a scope if not executing inside one.
		/// </summary>
		/// <param name="container">The <see cref="IWindsorContainer"/>.</param>
		/// <returns>Returns an <see cref="IDisposable"/> to be used in a <code>using</code> block.</returns>
		public static IDisposable RequireScope(this IWindsorContainer container)
		{
			return RequireScope();
		}

		private static IDisposable BeginScope()
		{
			return new CallContextLifetimeScope();
		}

		private static IDisposable RequireScope()
		{
			var current = CallContextLifetimeScope.ObtainCurrentScope();
			if (current == null)
			{
				return BeginScope();
			}
			return null; // Return null, not the parent otherwise you'll cause premature disposal
		}
	}
}