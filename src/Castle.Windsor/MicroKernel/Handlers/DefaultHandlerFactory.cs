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

namespace Castle.MicroKernel.Handlers
{
	using System;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ModelBuilder;

	[Serializable]
	public class DefaultHandlerFactory : IHandlerFactory
	{
		private readonly IKernelInternal kernel;

		public DefaultHandlerFactory(IKernelInternal kernel)
		{
			this.kernel = kernel;
		}

		public virtual IHandler Create(ComponentModel model)
		{
			var handler = CreateHandler(model);
			handler.Init(kernel);
			return handler;
		}

		private IHandler CreateHandler(ComponentModel model)
		{
			if (model.RequiresGenericArguments)
			{
				var matchingStrategy = GenericImplementationMatchingStrategy(model);
				var serviceStrategy = GenericServiceStrategy(model);
				return new DefaultGenericHandler(model, matchingStrategy, serviceStrategy);
			}

			// meta descriptors only apply to open generic handlers so we cam safely let go of them, save some memory
			ComponentModelDescriptorUtil.RemoveMetaDescriptors(model);

			var resolveExtensions = model.ResolveExtensions(false);
			var releaseExtensions = model.ReleaseExtensions(false);
			if (releaseExtensions == null && resolveExtensions == null)
			{
				return new DefaultHandler(model);
			}
			return new ExtendedHandler(model, resolveExtensions, releaseExtensions);
		}

		private IGenericImplementationMatchingStrategy GenericImplementationMatchingStrategy(ComponentModel model)
		{
			return (IGenericImplementationMatchingStrategy) model.ExtendedProperties[Constants.GenericImplementationMatchingStrategy];
		}

		private IGenericServiceStrategy GenericServiceStrategy(ComponentModel model)
		{
			return (IGenericServiceStrategy) model.ExtendedProperties[Constants.GenericServiceStrategy];
		}
	}
}