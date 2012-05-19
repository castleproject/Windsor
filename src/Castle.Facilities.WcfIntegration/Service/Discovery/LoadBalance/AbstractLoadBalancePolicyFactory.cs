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
#if DOTNET40
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.ServiceModel.Discovery;

    public abstract class AbstractLoadBalancePolicyFactory<TPolicy> : ILoadBalancePolicyFactory
		where TPolicy : class, ILoadBalancePolicy
    {
		static readonly PolicyCreator creator;
		static readonly Type[] CreateArgs = new[] { typeof(PolicyMembership) };
		const BindingFlags CreateFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public delegate TPolicy PolicyCreator(PolicyMembership membership);

		public ILoadBalancePolicy[] CreatePolicies(EndpointDiscoveryMetadata endpoint)
		{
			return CreatePolicies(endpoint, creator);
		}

		protected abstract ILoadBalancePolicy[] CreatePolicies(EndpointDiscoveryMetadata endpoint, PolicyCreator creator);

		private static ConstructorInfo GetPolicyConstructor()
		{
			var constractCtor = typeof(TPolicy).GetConstructor(CreateFlags, null, CreateArgs, null);

			if (constractCtor == null)
			{
				throw new InvalidOperationException(string.Format(
					"The policy type {0} does not have a constructor that accepts an {1}.  " +
				    "This is required for contractual based policy load balancing.",
					typeof(TPolicy).FullName, typeof(PolicyMembership).FullName));
			}

			return constractCtor;
		}

		static AbstractLoadBalancePolicyFactory()
		{
			var policyType = typeof(TPolicy);
			var policyCtor = GetPolicyConstructor();
			var dynamicMethod = new DynamicMethod("", policyType, CreateArgs, policyType);
			var ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Newobj, policyCtor);
			ilGenerator.Emit(OpCodes.Ret);
			creator = (PolicyCreator)dynamicMethod.CreateDelegate(typeof(PolicyCreator));
		}
    }
#endif
}
