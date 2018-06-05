// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Diagnostics.Helpers
{
	using System.Collections.Generic;
	using System.Text;

	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.Windsor.Diagnostics.DebuggerViews;

	public class DefaultComponentViewBuilder : IComponentDebuggerExtension
	{
		private readonly IHandler handler;

		public DefaultComponentViewBuilder(IHandler handler)
		{
			this.handler = handler;
		}

		public IEnumerable<object> Attach()
		{
			yield return new DebuggerViewItem("Implementation", GetImplementation());
			foreach (var service in handler.ComponentModel.Services)
			{
				yield return new DebuggerViewItem("Service", service);
			}
			yield return GetStatus();
			yield return new DebuggerViewItem("Lifestyle", handler.ComponentModel.GetLifestyleDescriptionLong());
			if (HasInterceptors())
			{
				var interceptors = handler.ComponentModel.Interceptors;
				var value = interceptors.ToArray();
				yield return new DebuggerViewItem("Interceptors", "Count = " + value.Length, value);
			}
			yield return new DebuggerViewItem("Name", handler.ComponentModel.Name);
			yield return new DebuggerViewItem("Raw handler/component", handler);
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

		private object GetStatus()
		{
			if (handler.CurrentState == HandlerState.Valid)
			{
				return new DebuggerViewItem("Status", "All required dependencies can be resolved.");
			}
			return new DebuggerViewItemWithDetails("Status", "This component may not resolve properly.", GetStatusDetails(handler as IExposeDependencyInfo));
		}

		private string GetStatusDetails(IExposeDependencyInfo info)
		{
			var message = new StringBuilder("Some dependencies of this component could not be statically resolved.");
			if (info == null)
			{
				return message.ToString();
			}
			var inspector = new DependencyInspector(message);
			info.ObtainDependencyDetails(inspector);

			return inspector.Message;
		}

		private bool HasInterceptors()
		{
			return handler.ComponentModel.HasInterceptors;
		}
	}
}