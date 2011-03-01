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

namespace Castle.MicroKernel.Handlers
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel.Util;

	public class DependencyInspector : IDependencyInspector
	{
		private readonly HashSet<IHandler> handlersChecked = new HashSet<IHandler>();

		private readonly StringBuilder message;

		public DependencyInspector(StringBuilder message)
		{
			this.message = message;
		}

		public string Message
		{
			get { return message.ToString(); }
		}

		public void Inspect(IHandler handler, DependencyModel[] missingDependencies, IKernel kernel)
		{
			if (handlersChecked.Add(handler) == false)
			{
				return;
			}
			Debug.Assert(missingDependencies.Length > 0, "missingDependencies.Length > 0");

			message.AppendLine();
			message.AppendFormat("'{0}' is waiting for the following dependencies:", handler.ComponentModel.Name);
			message.AppendLine();
			foreach (var dependency in missingDependencies)
			{
				if (string.IsNullOrEmpty(dependency.DependencyKey))
				{
					InspectServiceDependency(handler, dependency, kernel);
				}
				else if (ReferenceExpressionUtil.IsReference(dependency.DependencyKey))
				{
					InspectServiceOverrideDependency(dependency, kernel);
				}
				else
				{
					InspectParameterDependency(dependency);
				}
			}
		}

		private void InspectParameterDependency(DependencyModel dependency)
		{
			var key = dependency.DependencyKey;
			message.AppendFormat("- Parameter '{0}' which was not provided. Did you forget to set the dependency?", key);
			message.AppendLine();
		}

		private void InspectServiceDependency(IHandler inspectingHandler, DependencyModel dependency, IKernel kernel)
		{
			var type = dependency.TargetItemType;
			var handler = kernel.GetHandler(type);
			if (handler == null)
			{
				message.AppendFormat("- Service '{0}' which was not registered.", type.FullName ?? type.Name);
				message.AppendLine();
			}
			else if (handler == inspectingHandler)
			{
				var alternatives = kernel.GetHandlers(type);
				message.AppendFormat("- Service '{0}' which points back to the component itself.", type.FullName ?? type.Name);
				message.AppendLine();
				message.Append("A dependency cannot be satisfied by the component itself, did you forget to ");
				if (alternatives.Length == 1)
				{
					message.AppendLine("register other components for this service?");
				}
				else
				{
					message.AppendLine("make this a service override and point explicitly to a different component exposing this service?");
					message.AppendLine();
					message.Append("The following components also expose the service, but none of them can be resolved:");
					foreach (var maybeDecoratedHandler in alternatives.Where(maybeDecoratedHandler => maybeDecoratedHandler != inspectingHandler))
					{
						var info = maybeDecoratedHandler as IExposeDependencyInfo;
						if (info != null)
						{
							info.ObtainDependencyDetails(this);
						}
						else
						{
							message.AppendLine();
							message.AppendFormat("'{0}' is registered and is matching the required service, but cannot be resolved.",
							                     maybeDecoratedHandler.ComponentModel.Name);
						}
					}
				}
			}
			else
			{
				message.AppendFormat("- Service '{0}' which was registered but is also waiting for dependencies.", handler.ComponentModel.Name);
				var info = handler as IExposeDependencyInfo;
				if (info != null)
				{
					info.ObtainDependencyDetails(this);
				}
			}
		}

		private void InspectServiceOverrideDependency(DependencyModel dependency, IKernel kernel)
		{
			var key = dependency.DependencyKey;
			var handler = kernel.GetHandler(key);

			//TODO: what about self dependency?
			if (handler == null)
			{
				message.AppendFormat("- Service '{0}' (via override) which was not registered. Did you misspell the name?", key);
				message.AppendLine();
			}
			else
			{
				message.AppendFormat("- Service '{0}' (via override) which was registered but is also waiting for dependencies.", key);
				message.AppendLine();

				var info = handler as IExposeDependencyInfo;
				if (info != null)
				{
					info.ObtainDependencyDetails(this);
				}
			}
		}
	}
}