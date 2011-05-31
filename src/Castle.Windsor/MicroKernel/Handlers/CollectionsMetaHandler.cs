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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ComponentActivator;

	public class CollectionsMetaHandler : IMetaHandler, IDisposable
	{
		private static readonly MethodInfo buildHandlerGeneric = typeof(CollectionsMetaHandler).GetMethod("BuildHandlerGeneric",
		                                                                                                  BindingFlags.Static | BindingFlags.NonPublic);

		private readonly SimpleThreadSafeDictionary<Type, IHandler> innerHandlers = new SimpleThreadSafeDictionary<Type, IHandler>();
		private IKernelInternal kernel;

		public void Dispose()
		{
			var handlers = innerHandlers.YieldAllValues();
			foreach (var handler in handlers.OfType<IDisposable>())
			{
				handler.Dispose();
			}
		}

		public IHandler GetHandler(Type service, IHandler[] existingHandlers)
		{
			if (existingHandlers.Length > 0)
			{
				return null;
			}
			var type = service.GetCompatibleArrayItemType();
			if (type == null)
			{
				return null;
			}
			return innerHandlers.GetOrAdd(type, BuildHandler);
		}

		public void Init(IKernelInternal kernel)
		{
			this.kernel = kernel;
		}

		private IHandler BuildHandler(Type type)
		{
			return (IHandler)buildHandlerGeneric.MakeGenericMethod(type).Invoke(this, null);
		}

		private IHandler BuildHandlerGeneric<T>()
		{
			var model = kernel.ComponentModelBuilder.BuildModel(
				new ComponentName("castle.auto-collection: " + typeof(T).ToCSharpString(), false),
				new[]
				{
					typeof(IEnumerable<T>),
					typeof(ICollection<T>),
					typeof(IList<T>),
					typeof(T[])
				},
				typeof(T[]),
				null);
			if (model.LifestyleType == LifestyleType.Undefined)
			{
				model.LifestyleType = LifestyleType.Transient;
			}
			if (model.CustomLifestyle == null)
			{
				model.CustomComponentActivator = typeof(CollectionActivator<T>);
			}
			return kernel.AddCustomComponent(model, isMetaHandler: true);
		}
	}
}