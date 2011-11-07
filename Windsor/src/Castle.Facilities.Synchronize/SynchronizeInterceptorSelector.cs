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
	using System.Linq;
	using System.Reflection;

	using Castle.DynamicProxy;

	/// <summary>
	///   Selects which methods require synchronization.
	/// </summary>
	[Serializable]
	public class SynchronizeInterceptorSelector : IInterceptorSelector
	{
		private readonly IInterceptorSelector existingSelector;
		private readonly SynchronizeMetaInfo metaInfo;

		/// <summary>
		///   Constructs the selector with the existing selector.
		/// </summary>
		/// <param name = "metaInfo">The sync metadata.</param>
		/// <param name = "existingSelector">The existing selector.</param>
		public SynchronizeInterceptorSelector(SynchronizeMetaInfo metaInfo,
		                                      IInterceptorSelector existingSelector)
		{
			this.metaInfo = metaInfo;
			this.existingSelector = existingSelector;
		}

		/// <summary>
		/// </summary>
		/// <param name = "obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			var otherSelector = obj as SynchronizeInterceptorSelector;
			if (ReferenceEquals(otherSelector, null) ||
			    ReferenceEquals(metaInfo, otherSelector.metaInfo) == false)
			{
				return false;
			}

			if (!Equals(existingSelector, otherSelector.existingSelector))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			var result = metaInfo.GetHashCode();
			result = 29*result + (existingSelector != null ? existingSelector.GetHashCode() : 0);
			return result;
		}

		/// <summary>
		/// </summary>
		/// <param name = "type"></param>
		/// <param name = "method"></param>
		/// <param name = "interceptors"></param>
		/// W
		/// <returns></returns>
		public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
		{
			if (!metaInfo.Contains(method))
			{
				if (IsInterfaceMappingCandidate(type, method))
				{
					var map = type.GetInterfaceMap(method.DeclaringType);
					var index = Array.IndexOf(map.InterfaceMethods, method);
					if (index >= 0 && metaInfo.Contains(map.TargetMethods[index]))
					{
						return interceptors;
					}
				}

				interceptors = interceptors.Where(i => i is SynchronizeInterceptor == false).ToArray();
			}

			if (existingSelector != null)
			{
				interceptors = existingSelector.SelectInterceptors(type, method, interceptors);
			}

			return interceptors;
		}

		private static bool IsInterfaceMappingCandidate(Type type, MemberInfo method)
		{
			return (type != method.DeclaringType && method.DeclaringType.IsInterface);
		}
	}
}