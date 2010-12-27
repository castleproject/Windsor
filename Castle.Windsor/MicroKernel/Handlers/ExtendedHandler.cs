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
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	public class ExtendedHandler : DefaultHandler
	{
		private readonly IReleaseExtension[] releaseExtensions;
		private readonly IResolveExtension[] resolveExtensions;

		public ExtendedHandler(ComponentModel model, ICollection<IResolveExtension> resolveExtensions,
		                       ICollection<IReleaseExtension> releaseExtensions)
			: base(model)
		{
			if (resolveExtensions != null)
			{
				this.resolveExtensions = resolveExtensions.ToArray();
			}
			if (releaseExtensions != null)
			{
				this.releaseExtensions = releaseExtensions.ToArray();
			}
		}

		public override void Init(IKernel kernel)
		{
			base.Init(kernel);

			if (resolveExtensions != null)
			{
				foreach (var extension in resolveExtensions)
				{
					extension.Init(kernel, this);
				}
			}
			if (releaseExtensions != null)
			{
				foreach (var extension in releaseExtensions)
				{
					extension.Init(kernel, this);
				}
			}
		}

		public override bool Release(Burden burden)
		{
			if (releaseExtensions == null)
			{
				return base.Release(burden);
			}

			var invocation = new ReleaseInvocation(burden);
			InvokeReleasePipeline(0, invocation);
			return invocation.ReturnValue;
		}

		protected override object Resolve(CreationContext context, bool instanceRequired)
		{
			if (resolveExtensions == null)
			{
				return base.Resolve(context, instanceRequired);
			}
			var invocation = new ResolveInvocation(context, instanceRequired);
			InvokeResolvePipeline(0, invocation);
			return invocation.ResolvedInstance;
		}

		private void InvokeReleasePipeline(int extensionIndex, ReleaseInvocation invocation)
		{
			if (extensionIndex >= releaseExtensions.Length)
			{
				invocation.ReturnValue = base.Release(invocation.Burden);
				return;
			}
			var nextIndex = extensionIndex + 1;
			invocation.SetProceedDelegate(() => InvokeReleasePipeline(nextIndex, invocation));
			releaseExtensions[extensionIndex].Intercept(invocation);
		}

		private void InvokeResolvePipeline(int extensionIndex, ResolveInvocation invocation)
		{
			if (extensionIndex >= resolveExtensions.Length)
			{
				invocation.ResolvedInstance = ResolveCore(invocation.Context,
				                                          invocation.DecommissionRequired,
				                                          invocation.InstanceRequired);
				return;
			}
			var nextIndex = extensionIndex + 1;
			invocation.SetProceedDelegate(() => InvokeResolvePipeline(nextIndex, invocation));
			resolveExtensions[extensionIndex].Intercept(invocation);
		}
	}
}