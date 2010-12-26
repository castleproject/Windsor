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

namespace Castle.MicroKernel.Tests.Lifestyle
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Lifestyle;

	public class Scope : IResolveExtension
	{
		public void Init(IKernel kernel, IHandler handler)
		{
		}

		public void Intercept(ResolveInvocation invocation)
		{
			using (new InstanceScope())
			{
				invocation.Proceed();
			}
		}
	}

	public class ScopedLifestyle : AbstractLifestyleManager
	{
		public override void Dispose()
		{
		}

		protected override Burden CreateInstance(CreationContext context, bool trackedExternally)
		{
			var scope = InstanceScope.Current;
			if (scope == null)
			{
				throw new InvalidOperationException("Scope is null");
			}

			Burden instance;
			if (scope.Cache.TryGetValue(Model, out instance) == false)
			{
				instance = base.CreateInstance(context, trackedExternally);
				scope.Cache[Model] = instance;
			}
			return instance;
		}
	}

	public class InstanceScope : IDisposable
	{
		[ThreadStatic]
		private static Stack<InstanceScope> localScopes;

		private IDictionary<ComponentModel, Burden> cache = new Dictionary<ComponentModel, Burden>();

		public InstanceScope()
		{
			if (localScopes == null)
			{
				localScopes = new Stack<InstanceScope>();
			}
			localScopes.Push(this);
		}

		public IDictionary<ComponentModel, Burden> Cache
		{
			get { return cache; }
		}

		public void Dispose()
		{
			localScopes.Pop();
		}

		public static InstanceScope Current
		{
			get
			{
				if (localScopes == null || localScopes.Count == 0)
				{
					return null;
				}
				return localScopes.Peek();
			}
		}
	}
}