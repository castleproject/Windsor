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

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Linq;
	using System.Threading;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle.Scoped;

	[Serializable]
	public class ThreadScopeAccessor : IScopeAccessor
	{
		private readonly SimpleThreadSafeDictionary<int, ILifetimeScope> items = new SimpleThreadSafeDictionary<int, ILifetimeScope>();

		public void Dispose()
		{
			var values = items.EjectAllValues();
			foreach (var item in values.Reverse())
			{
				item.Dispose();
			}
		}

		public ILifetimeScope GetScope(CreationContext context)
		{
			var currentThreadId = GetCurrentThreadId();
			return items.GetOrAdd(currentThreadId, id => new DefaultLifetimeScope());
		}

		protected virtual int GetCurrentThreadId()
		{
			return Thread.CurrentThread.ManagedThreadId;
		}
	}
}