// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

	/// <summary>
	/// Extended contract of kernel, used internally.
	/// </summary>
	public interface IKernelInternal : IKernel
	{
		/// <summary>
		/// Constructs an implementation of <see cref="IComponentActivator"/>
		/// for the given <see cref="ComponentModel"/>
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		IComponentActivator CreateComponentActivator(ComponentModel model);

		IHandler LoadHandlerByKey(string key, Type service, IDictionary arguments);

		IHandler LoadHandlerByType(string key, Type service, IDictionary arguments);

		/// <summary>
		/// Raise the handler registered event, required so
		/// dependant handlers will be notified about their dependant moving
		/// to valid state.
		/// </summary>
		/// <param name="handler"></param>
		void RaiseHandlerRegistered(IHandler handler);

		void RaiseHandlersChanged();

		/// <summary>
		/// Adds a custom made <see cref="ComponentModel"/>.
		/// Used by facilities.
		/// </summary>
		/// <param name="model"></param>
		void AddCustomComponent(ComponentModel model);

		IDisposable OptimizeDependencyResolution();
	}
}