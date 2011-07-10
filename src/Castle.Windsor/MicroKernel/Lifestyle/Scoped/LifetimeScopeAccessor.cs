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

	using Castle.MicroKernel.Context;

	public class LifetimeScopeAccessor : ICurrentScopeAccessor
	{
		public IScope2 GetScope(CreationContext context, bool required = true)
		{
			var scope = LifetimeScope.ObtainCurrentScope();
			if (scope == null)
			{
				if (required)
				{
					throw new InvalidOperationException("Scope not found. Make this exception message better");
				}
				return null;
			}
			return scope;
		}

		public void Dispose()
		{
			var scope = LifetimeScope.ObtainCurrentScope();
			if (scope != null)
			{
				scope.Dispose();
			}
		}
	}
}