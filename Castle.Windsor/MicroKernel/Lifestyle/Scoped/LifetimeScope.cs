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

namespace Castle.MicroKernel.Lifestyle.Scoped
{
	using System;
	using System.Runtime.Remoting.Messaging;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.Windsor;

	public class LifetimeScope : IScope2
	{
		private readonly Lock @lock = Lock.Create();
		private readonly string name;

		public LifetimeScope(IKernel container)
		{
			name = Guid.NewGuid().ToString();
		}

		public LifetimeScope(IWindsorContainer container) : this(container.Kernel)
		{
		}

		public void Dispose()
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				var data = (LogicalThreadAffinativeScopeCache)CallContext.GetData(name);
				if (data == null)
				{
					return;
				}
				token.Upgrade();
				data.Dispose();
				CallContext.FreeNamedDataSlot(name);
			}
		}

		public Burden GetCachedInstance(ComponentModel instance, Func<Burden> instanceNotFoundCallback)
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				var data = (LogicalThreadAffinativeScopeCache)CallContext.GetData(name) ?? new LogicalThreadAffinativeScopeCache();
				var burden = data[instance];
				if (burden == null)
				{
					token.Upgrade();

					burden = instanceNotFoundCallback();
					data[instance] = burden;
					CallContext.SetData(name, data);
				}
				return burden;
			}
		}

		public static LifetimeScope ObtainCurrentScope()
		{
			return (LifetimeScope)CallContext.GetData("Castle.Lifetime-scope");
		}
	}
}