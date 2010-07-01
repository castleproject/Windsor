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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;

	/// <summary>
	/// Summary description for DefaultHandlerFactory.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class DefaultHandlerFactory : IHandlerFactory
	{
		private readonly IKernel kernel;

		public DefaultHandlerFactory(IKernel kernel)
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
				return new DefaultGenericHandler(model);
			}
			var resolveExtensions = model.ExtendedProperties["Castle.ResolveExtensions"] as ICollection<IResolveExtension>;
			var releaseExtensions = model.ExtendedProperties["Castle.ReleaseExtensions"] as ICollection<IReleaseExtension>;
			if (releaseExtensions == null && resolveExtensions == null)
			{
				return new DefaultHandler(model);
			}
			return new ExtendedHandler(model, resolveExtensions, releaseExtensions);
		}

		public IHandler CreateForwarding(IHandler target, Type forwardedType)
		{
			return new ForwardingHandler(target, forwardedType);
		}
	}
}