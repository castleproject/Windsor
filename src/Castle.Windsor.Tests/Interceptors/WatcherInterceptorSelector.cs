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

#if !SILVERLIGHT
namespace Castle.Windsor.Tests.Interceptors
{
	using Castle.Core;
	using Castle.MicroKernel.Proxy;

	public class WatcherInterceptorSelector : IModelInterceptorsSelector
	{
		public InterceptorKind Interceptors = InterceptorKind.None;

		public bool HasInterceptors(ComponentModel model)
		{
			return model.Service == typeof(IWatcher) && Interceptors == InterceptorKind.Dummy;
		}

		public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
		{
			if (model.Service != typeof(IWatcher))
			{
				return null;
			}
			if (Interceptors == InterceptorKind.None)
			{
				return null;
			}
			return new[] { new InterceptorReference(typeof(WasCalledInterceptor)), };
		}
	}
}
#endif