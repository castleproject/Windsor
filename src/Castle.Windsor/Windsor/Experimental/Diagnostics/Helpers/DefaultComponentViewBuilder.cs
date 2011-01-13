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

namespace Castle.Windsor.Experimental.Diagnostics.Helpers
{
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.Windsor.Experimental.Diagnostics.DebuggerViews;

#if !SILVERLIGHT
	public class DefaultComponentViewBuilder : IComponentDebuggerExtension
	{
		private readonly IHandler handler;

		public DefaultComponentViewBuilder(IHandler handler)
		{
			this.handler = handler;
		}

		public IEnumerable<DebuggerViewItem> Attach()
		{
			yield return new DebuggerViewItem("Implementation", GetImplementation());
			foreach (var service in handler.Services)
			{
				yield return new DebuggerViewItem("Service", service);
			}
			yield return new DebuggerViewItem("Status", GetStatus());
			yield return new DebuggerViewItem("Lifestyle", GetLifestyleDescription(handler.ComponentModel));
			if (HasInterceptors())
			{
				var interceptors = handler.ComponentModel.Interceptors;
				var value = interceptors.ToArray();
				yield return new DebuggerViewItem("Interceptors", "Count = " + value.Length, value);
			}
			yield return new DebuggerViewItem("Raw handler", handler);
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

		// TODO: this should go to the description util
		private object GetLifestyleDescription(ComponentModel componentModel)
		{
			var lifestyle = componentModel.LifestyleType;
			if (lifestyle == LifestyleType.Custom)
			{
				return "Custom: " + componentModel.CustomLifestyle.Name;
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
			return handler.ComponentModel.HasInterceptors;
		}
	}
#endif
}