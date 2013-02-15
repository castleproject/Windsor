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

namespace Castle.Facilities.WcfIntegration
{
#if !DOTNET35
	public class RoundRobinPolicy : ListBasedLoadBalancePolicy
	{
		public RoundRobinPolicy(PolicyMembership membership)
			: base(membership)
		{
		}

		protected override void ChooseTarget(ChooseContext choose)
		{
			choose.ModifyList(targets =>
			{
				var count = targets.Count;
				for (int index = 0; index < count; ++index)
				{
					var target = targets[index];
					if (choose.Matches(target))
					{
						if (count > 1)
						{
							targets.RemoveAt(index);
							targets.Add(target);
						}
						return target;
					}
				}
				return null;
			});
		}
	}
#endif
}