// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

#if NET45

namespace Castle.Facilities.Owin.WebHost.Lifestyles
{
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal class MsSystemWebHttpContextScopeAccessor : IScopeAccessor
	{
		private const string ScopeKey = "castle.windsor.facility.owin.scope";

		public ILifetimeScope GetScope(CreationContext context)
		{
			return RequireScope();
		}

		public static ILifetimeScope RequireScope()
		{
			return MsSystemWebHttpContextScopeWrapper.GetOrSet<ILifetimeScope>(ScopeKey, () => new MsSystemWebHttpContextLifetimeScope());
		}

		public static void ReleaseScope()
		{
			MsSystemWebHttpContextScopeWrapper.Get<ILifetimeScope>(ScopeKey).Dispose();
		}

		public void Dispose()
		{
			ReleaseScope();
		}
	}
}

#endif