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

namespace Castle.MicroKernel
{
	using System;
	using System.Collections;

	using Castle.Core;
	using Castle.Core.Logging;

	/// <summary>
	/// Extended contract of kernel, used internally.
	/// </summary>
	public interface IKernelInternal : IKernel
	{
		/// <summary>
		/// Internal logger used by the container (not just <see cref = "IKernelInternal" /> implementation itself but also other classes too) to log information about what's happening in the container.
		/// </summary>
		ILogger Logger { get; set; }

		/// <summary>
		/// Adds a custom made <see cref = "ComponentModel" />. Used by facilities.
		/// </summary>
		/// <param name = "model"> </param>
		IHandler AddCustomComponent(ComponentModel model);

		/// <summary>
		/// Constructs an implementation of <see cref = "IComponentActivator" /> for the given <see cref = "ComponentModel" />
		/// </summary>
		/// <param name = "model"> </param>
		/// <returns> </returns>
		IComponentActivator CreateComponentActivator(ComponentModel model);

		ILifestyleManager CreateLifestyleManager(ComponentModel model, IComponentActivator activator);

		IHandler LoadHandlerByName(string key, Type service, IDictionary arguments);

		IHandler LoadHandlerByType(string key, Type service, IDictionary arguments);

		IDisposable OptimizeDependencyResolution();

		object Resolve(Type service, IDictionary arguments, IReleasePolicy policy);

		/// <summary>
		/// Returns a component instance by the key
		/// </summary>
		/// <param name = "key"> </param>
		/// <param name = "service"> </param>
		/// <param name = "arguments"> </param>
		/// <param name = "policy"> </param>
		/// <returns> </returns>
		object Resolve(String key, Type service, IDictionary arguments, IReleasePolicy policy);

		Array ResolveAll(Type service, IDictionary arguments, IReleasePolicy policy);

		IHandler CreateHandler(ComponentModel model);

		void RaiseEventsOnHandlerCreated(IHandler handler);
	}
}