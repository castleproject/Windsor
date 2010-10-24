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

namespace Castle.Windsor.Experimental.Debugging.Primitives
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;

#if !SILVERLIGHT
	public class DefaultComponentView : IComponentDebuggerExtension
	{
		private readonly IEnumerable<Type> forwardedTypes;
		private readonly IHandler handler;

		public DefaultComponentView(IHandler handler, IEnumerable<Type> forwardedTypes)
		{
			this.handler = handler;
			this.forwardedTypes = forwardedTypes;
		}

		private object GetImplementation()
		{
			var implementation = handler.ComponentModel.Implementation;
			if (implementation != typeof(LateBoundComponent))
			{
				return implementation;
			}

			return LateBoundComponent.Instance;
		}

		private object GetLifestyle()
		{
			var lifestyle = handler.ComponentModel.LifestyleType;
			if (lifestyle == LifestyleType.Custom)
			{
				return "Custom: " + handler.ComponentModel.CustomLifestyle.Name;
			}
			if (lifestyle == LifestyleType.Undefined)
			{
				return string.Format("{0} (default lifestyle {1} will be used)", lifestyle, LifestyleType.Singleton);
			}
			return lifestyle;
		}

		private object GetStatus()
		{
			if (handler.CurrentState == HandlerState.Valid)
			{
				return "All required dependencies can be resolved.";
			}
			return new ComponentStatusDebuggerViewItem(handler as IExposeDependencyInfo);
		}

		private bool HasInterceptors()
		{
			return handler.ComponentModel.Interceptors.HasInterceptors;
		}

		public IEnumerable<DebuggerViewItem> Attach()
		{
			yield return new DebuggerViewItem("Implementation", GetImplementation());
			yield return new DebuggerViewItem("Service", handler.Service);
			foreach (var forwardedType in forwardedTypes)
			{
				yield return new DebuggerViewItem("Service", forwardedType);
			}
			yield return new DebuggerViewItem("Status", GetStatus());
			yield return new DebuggerViewItem("Lifestyle", GetLifestyle());
			if (HasInterceptors())
			{
				var interceptors = handler.ComponentModel.Interceptors;
				yield return
					new DebuggerViewItem("Interceptors", interceptors.ToArray());
			}
			yield return new DebuggerViewItem("Raw handler", handler);
		}
	}
#endif
}