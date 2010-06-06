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

		bool LazyLoadComponentByKey(string key, Type service);

		bool LazyLoadComponentByType(string key, Type service);

		/// <summary>
		/// Raise the hanlder registered event, required so
		/// dependant handlers will be notified about their dependant moving
		/// to valid state.
		/// </summary>
		/// <param name="handler"></param>
		void RaiseHandlerRegistered(IHandler handler);

		void RaiseHandlersChanged();

		/// <summary>
		/// Registers the <paramref name="forwardedType"/> to be forwarded 
		/// to the component registered with <paramref name="name"/>.
		/// </summary>
		/// <param name="forwardedType">The service type that gets forwarded.</param>
		/// <param name="name">The name of the component to forward to.</param>
		void RegisterHandlerForwarding(Type forwardedType, string name);
	}
}