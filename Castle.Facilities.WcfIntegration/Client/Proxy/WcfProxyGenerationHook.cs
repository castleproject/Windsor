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

namespace Castle.Facilities.WcfIntegration.Proxy
{
	using System;
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.DynamicProxy;

	[Serializable]
	public class WcfProxyGenerationHook : IProxyGenerationHook
	{
		private readonly IProxyGenerationHook hook;

		public WcfProxyGenerationHook(IProxyGenerationHook hook)
		{
			this.hook = hook;
		}



		public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
		{
			if (IsChannelHolderMethod(methodInfo))
			{
				return false;
			}

			if (hook != null)
			{
				return hook.ShouldInterceptMethod(type, methodInfo);
			}

			return true;
		}

		private bool IsChannelHolderMethod(MethodInfo methodInfo)
		{
			return methodInfo.DeclaringType.Is<IWcfChannelHolder>();
		}

		public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
		{
			if (hook != null)
			{
				//give the inner hook a chance to throw its own exception
				hook.NonProxyableMemberNotification(type, memberInfo);
			}

			// actually we should never get this, since we're doing an interface proxy
			// so if we do, this may mean it's a bug.
			throw new NotSupportedException(
				string.Format("Member {0}.{1} is non virtual hence can not be proxied. If you think it's a bug, please report it.",
				              type.FullName, memberInfo.Name));
		}

		public void MethodsInspected()
		{
			if (hook!=null)
			{
				hook.MethodsInspected();
			}
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(WcfProxyGenerationHook))
			{
				return false;
			}

			return Equals(((WcfProxyGenerationHook)obj).hook, hook);
		}

		public override int GetHashCode()
		{
			return (hook != null ? hook.GetHashCode() : 0);
		}
	}
}