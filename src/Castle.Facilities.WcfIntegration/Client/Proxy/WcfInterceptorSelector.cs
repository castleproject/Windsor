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
	using System.Collections.Generic;
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using Castle.DynamicProxy;

	[Serializable]
	public class WcfInterceptorSelector : IInterceptorSelector
	{
		private readonly Type proxiedType;
		private readonly IInterceptorSelector userProvidedSelector;

		public WcfInterceptorSelector(Type proxiedType, IInterceptorSelector userProvidedSelector)
		{
			this.proxiedType = proxiedType;
			this.userProvidedSelector = userProvidedSelector;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			var wcfInterceptorSelector = obj as WcfInterceptorSelector;

			if (ReferenceEquals(wcfInterceptorSelector, null))
				return false;

			if (Equals(proxiedType, wcfInterceptorSelector.proxiedType) == false)
				return false;

			if (Equals(userProvidedSelector, wcfInterceptorSelector.userProvidedSelector) == false)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			var result = proxiedType != null ? proxiedType.GetHashCode() : 0;
			result = 29 * result + (userProvidedSelector != null ? userProvidedSelector.GetHashCode() : 0);
			return result;
		}

		public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
		{
			if (IsProxiedTypeMethod(method))
				return SelectInterceptorsForProxiedType(method, interceptors, type);

			if (IsServiceMethod(method))
				return SelectInterceptorsForServiceType(method, interceptors);


			return interceptors;
		}

		private static IInterceptor[] SelectInterceptorsForServiceType(MethodInfo method, IInterceptor[] interceptors)
		{
			return Array.FindAll(interceptors, i => i is IWcfInterceptor);
		}

		private IInterceptor[] SelectInterceptorsForProxiedType(MethodInfo method, IInterceptor[] interceptors, Type type)
		{
			List<IInterceptor> infrastructureInterceptors, userInterceptors;
			SplitInterceptors(interceptors, method, out infrastructureInterceptors, out userInterceptors);

			var selectedInterceptors = AddWcfInterceptors(infrastructureInterceptors,
				SelectUserInterceptors(method, userInterceptors, type));

			return selectedInterceptors;
		}

		private static void SplitInterceptors(IInterceptor[] interceptors, MethodInfo method,
											  out List<IInterceptor> infrastructureInterceptors,
											  out List<IInterceptor> userInterceptors)
		{
			userInterceptors = new List<IInterceptor>(interceptors.Length);
			infrastructureInterceptors = new List<IInterceptor>(interceptors.Length);

			foreach (var interceptor in interceptors)
			{
				if (interceptor is IWcfInterceptor)
				{
					var infrastructureInterceptor = (IWcfInterceptor)interceptor;
					if (infrastructureInterceptor.Handles(method))
					{
						infrastructureInterceptors.Add(infrastructureInterceptor);
					}
					continue;
				}
				userInterceptors.Add(interceptor);
			}
		}

		private IInterceptor[] SelectUserInterceptors(MethodInfo method, List<IInterceptor> userInterceptors, Type type)
		{
			var selectedInterceptors = userInterceptors.ToArray();

			if (userProvidedSelector != null)
				selectedInterceptors = userProvidedSelector.SelectInterceptors(type, method, selectedInterceptors);

			return selectedInterceptors;
		}

		private static IInterceptor[] AddWcfInterceptors(List<IInterceptor> infrastructureInterceptors,
														 IInterceptor[] selectedInterceptors)
		{
			if (infrastructureInterceptors.Count > 0)
			{
				var index = selectedInterceptors.Length;
				Array.Resize(ref selectedInterceptors, index + infrastructureInterceptors.Count);
				infrastructureInterceptors.CopyTo(selectedInterceptors, index);
			}

			return selectedInterceptors;
		}

		private static bool IsServiceMethod(MethodInfo method)
		{
			var type = method.DeclaringType;
			return type.IsAssignableFrom(typeof(IChannel)) ||
				   type.IsAssignableFrom(typeof(IClientChannel)) ||
			       type.IsAssignableFrom(typeof(IServiceChannel)) ||
			       type.IsAssignableFrom(typeof(IDuplexContextChannel));
		}

		private bool IsProxiedTypeMethod(MethodInfo method)
		{
			return method.DeclaringType.IsAssignableFrom(proxiedType);
		}
	}
}