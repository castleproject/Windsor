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

namespace Castle.Windsor.Debugging
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;

	public class DefaultComponentView : IComponentDebuggerExtensions
	{
		private readonly IHandler handler;

		public DefaultComponentView(IHandler handler)
		{
			this.handler = handler;
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

		private object GetStatusMessage()
		{
			if (handler.CurrentState == HandlerState.Valid)
			{
				return "All required dependencies can be resolved.";
			}

			var message = "Some dependencies of this component could not be statically resolved.";
			var info = handler as IExposeDependencyInfo;
			if (info == null)
			{
				return message;
			}
			return message + info.ObtainDependencyDetails(new List<object>());
		}

		public IEnumerable<IDebuggerViewItem> Attach()
		{
			yield return new DebuggerViewItem("Implementation", handler.ComponentModel.Implementation);
			yield return new DebuggerViewItem("Service", handler.Service);
			yield return new DebuggerViewItem("Status", GetStatusMessage());
			yield return new DebuggerViewItem("Lifestyle", GetLifestyle());
			if(HasInterceptors())
			{
				var interceptors = handler.ComponentModel.Interceptors;
				yield return new DebuggerViewItemWithDescribtion("Interceptors", "Count = " + interceptors.Count, interceptors);
			}
		}

		private bool HasInterceptors()
		{
			return handler.ComponentModel.Interceptors.HasInterceptors;
		}
	}
}