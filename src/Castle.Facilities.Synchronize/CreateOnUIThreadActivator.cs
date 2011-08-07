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

namespace Castle.Facilities.Synchronize
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;

	internal delegate object CreationContextDelegate(CreationContext context);

	internal delegate object CreateOnUIThreadDelegate(CreationContextDelegate create, CreationContext context);

	internal class CreateOnUIThreadActivator : DefaultComponentActivator
	{
		private static readonly ComponentInstanceDelegate emptyCreation = delegate { };
		private static readonly ComponentInstanceDelegate emptyDestruction = delegate { };
		private readonly IComponentActivator customActivator;
		private readonly CreationContextDelegate performCreation;

		/// <summary>
		///   Initializes a new instance of the <see cref = "CreateOnUIThreadActivator" /> class.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <param name = "kernel">The kernel.</param>
		/// <param name = "onCreation">Delegate called on construction.</param>
		/// <param name = "onDestruction">Delegate called on destruction.</param>
		public CreateOnUIThreadActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation,
		                                 ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
			customActivator = CreateCustomActivator(model, kernel);
			performCreation = PerformCreation;
		}

		protected override object InternalCreate(CreationContext context)
		{
			var createOnUIThread = (CreateOnUIThreadDelegate)Model.ExtendedProperties[Constants.CreateOnUIThead];

			if (createOnUIThread != null)
			{
				return createOnUIThread(performCreation, context);
			}

			return base.InternalCreate(context);
		}

		private object PerformCreation(CreationContext context)
		{
			if (customActivator != null)
			{
				return customActivator.Create(context);
			}

			return base.InternalCreate(context);
		}

		private static IComponentActivator CreateCustomActivator(ComponentModel model, IKernel kernel)
		{
			var customActivator = (Type)model.ExtendedProperties[Constants.CustomActivator];
			if (customActivator != null)
			{
				try
				{
					return (IComponentActivator)Activator.CreateInstance(
						customActivator, model, kernel, emptyCreation, emptyDestruction);
				}
				catch (Exception e)
				{
					throw new KernelException("Could not instantiate the custom activator", e);
				}
			}
			return null;
		}
	}
}