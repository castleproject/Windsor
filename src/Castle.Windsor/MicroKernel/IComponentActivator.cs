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

namespace Castle.MicroKernel
{
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Implements the instance creation logic. The default
	///   implementation should rely on an ordinary call to 
	///   Activator.CreateInstance().
	/// </summary>
	/// <remarks>
	///   This interface is provided in order to allow custom components
	///   to be created using a different logic, such as using a specific factory
	///   or builder.
	///   <para>
	///     The constructor for implementation has the following signature:
	///   </para>
	///   <code>
	///     ComponentModel model, IKernel kernel, 
	///     ComponentInstanceDelegate onCreation, 
	///     ComponentInstanceDelegate onDestruction
	///   </code>
	///   <para>
	///     The Activator should raise the events onCreation and onDestruction
	///     in order to correctly implement the contract. Usually the best
	///     way of creating a custom activator is by extending the existing ones.
	///   </para>
	///   <seealso cref = "ComponentActivator.AbstractComponentActivator" />
	///   <seealso cref = "ComponentActivator.DefaultComponentActivator" />
	/// </remarks>
	public interface IComponentActivator
	{
		/// <summary>
		///   Should return a new component instance.
		/// </summary>
		/// <returns></returns>
		object Create(CreationContext context, Burden burden);

		/// <summary>
		///   Should perform all necessary work to dispose the instance
		///   and/or any resource related to it.
		/// </summary>
		/// <param name = "instance"></param>
		void Destroy(object instance);
	}
}