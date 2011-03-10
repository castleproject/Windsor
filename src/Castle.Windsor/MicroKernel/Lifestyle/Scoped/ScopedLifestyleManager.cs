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
	using Castle.MicroKernel.Context;

	public class ScopedLifestyleManager : AbstractLifestyleManager
	{
		private readonly ICurrentScopeAccessor accessor;

		public ScopedLifestyleManager(ICurrentScopeAccessor accessor)
		{
			this.accessor = accessor;
		}

		public override void Dispose()
		{
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			var scope = accessor.GetScope(context);
			var burden = scope.GetComponentBurden(this);
			if (burden != null)
			{
				return burden.Instance;
			}
			burden = base.CreateInstance(context, trackedExternally: true);
			scope.AddComponent(this, burden);
			Track(burden, releasePolicy);
			return burden.Instance;
		}
	}
}