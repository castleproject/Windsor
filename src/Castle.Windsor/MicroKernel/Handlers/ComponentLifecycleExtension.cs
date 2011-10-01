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
	using System.Collections.Generic;

	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Util;

	public delegate ComponentReleasingDelegate ComponentResolvingDelegate(IKernel kernel, CreationContext context);

	public delegate void ComponentReleasingDelegate(IKernel kernel);

	public class ComponentLifecycleExtension : IResolveExtension, IReleaseExtension
	{
		private readonly object releaseInvocationLocker = new object();
		private IKernel kernel;
		private IDictionary<object, IList<ComponentReleasingDelegate>> releasingHandlers;
		private ComponentResolvingDelegate resolvingHandler;

		public void AddHandler(ComponentResolvingDelegate handler)
		{
			resolvingHandler += handler;
		}

		public void Intercept(ReleaseInvocation invocation)
		{
			IList<ComponentReleasingDelegate> releasers = null;
			lock (releaseInvocationLocker)
			{
				if (releasingHandlers != null && releasingHandlers.TryGetValue(invocation.Instance, out releasers))
				{
					releasingHandlers.Remove(invocation.Instance);
				}
			}

			if (releasers != null)
			{
				foreach (var releaser in releasers)
				{
					releaser(kernel);
				}
			}
			invocation.Proceed();
		}

		public void Init(IKernel kernel, IHandler handler)
		{
			this.kernel = kernel;
		}

		public void Intercept(ResolveInvocation invocation)
		{
			List<ComponentReleasingDelegate> releasers = null;
			if (resolvingHandler != null)
			{
				foreach (ComponentResolvingDelegate resolver in resolvingHandler.GetInvocationList())
				{
					var releaser = resolver(kernel, invocation.Context);
					if (releaser != null)
					{
						if (releasers == null)
						{
							releasers = new List<ComponentReleasingDelegate>();
							invocation.RequireDecommission();
						}
						releasers.Add(releaser);
					}
				}
			}

			invocation.Proceed();

			if (releasers == null)
			{
				return;
			}

			lock (releaseInvocationLocker)
			{
				if (releasingHandlers == null)
				{
					releasingHandlers = new Dictionary<object, IList<ComponentReleasingDelegate>>(new ReferenceEqualityComparer());
				}

				if (releasingHandlers.ContainsKey(invocation.ReturnValue) == false)
				{
					releasingHandlers.Add(invocation.ReturnValue, releasers);
				}
			}
		}
	}
}