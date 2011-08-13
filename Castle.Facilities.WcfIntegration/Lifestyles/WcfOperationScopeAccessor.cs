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

namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System.ServiceModel;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle.Scoped;

	public class WcfOperationScopeAccessor : IScopeAccessor
	{
		private ThreadSafeFlag disposed;

		public void Dispose()
		{
			if (disposed.Signal() == false)
			{
				return;
			}
			var channel = GetScopeHolder();
			if (channel == null)
			{
				return;
			}
			var extension = channel.Extensions.Find<WcfOperationScopeHolder>();
			if (extension != null && channel.Extensions.Remove(extension))
			{
				extension.Dispose();
			}
		}

		public ILifetimeScope GetScope(CreationContext context)
		{
			var scopeHolder = GetScopeHolder();
			return GetScope(scopeHolder);
		}

		private static ILifetimeScope GetScope(IExtensibleObject<OperationContext> scopeHolder)
		{
			if (scopeHolder == null)
			{
				return null;
			}
			var extension = scopeHolder.Extensions.Find<WcfOperationScopeHolder>();
			if (extension == null)
			{
				extension = new WcfOperationScopeHolder(new DefaultLifetimeScope());
				scopeHolder.Extensions.Add(extension);
			}
			return extension.Scope;
		}

		private static OperationContext GetScopeHolder()
		{
			return OperationContext.Current;
		}
	}
}