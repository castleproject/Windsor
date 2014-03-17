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

namespace Castle.Facilities.Synchronize
{
	using System;
	using System.Reflection;

	using Castle.DynamicProxy;

	/// <summary>
	///   Proxy generation hook to filter all System methods when proxying a control.
	/// </summary>
	public class SynchronizeProxyHook : IProxyGenerationHook
	{
		/// <summary>
		///   Singleton instance.
		/// </summary>
		public static readonly SynchronizeProxyHook Instance = new SynchronizeProxyHook();

		/// <summary>
		/// </summary>
		protected SynchronizeProxyHook()
		{
		}

		public override bool Equals(object obj)
		{
			return obj.GetType() == GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}

		/// <summary>
		///   Not used.
		/// </summary>
		public void MethodsInspected()
		{
		}

		/// <summary>
		///   Not used.
		/// </summary>
		/// <param name = "type"></param>
		/// <param name = "memberInfo"></param>
		public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
		{
		}

		/// <summary>
		///   Filters System methods.
		/// </summary>
		/// <param name = "type">The type.</param>
		/// <param name = "methodInfo">The method info.</param>
		/// <returns>true if not a System namespace, false otherwise.</returns>
		public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
		{
			return methodInfo.DeclaringType.Namespace.StartsWith("System.") == false;
		}
	}
}