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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core.Internal;
	using Castle.MicroKernel;

#if !SILVERLIGHT
	[DebuggerDisplay("{key} {Description,nq}")]
	public class ComponentDebuggerView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IComponentDebuggerExtension[] extension;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int additionalServicesCount;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler handler;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string key;

		public ComponentDebuggerView(IHandler handler, params IComponentDebuggerExtension[] defaultExtension)
		{
			key = handler.ComponentModel.Name;
			additionalServicesCount = handler.Services.Count() - 1;
			this.handler = handler;
			extension = defaultExtension.Concat(GetExtensions(handler)).ToArray();
			description = BuildDescription();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItem[] Extensions
		{
			get { return extension.SelectMany(e => e.Attach()).ToArray(); }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string Description
		{
			get { return description; }
		}

		private string BuildDescription()
		{
			var services = handler.Services.ToArray();
			var message = new StringBuilder(services[0].Name);
			if (additionalServicesCount == 1)
			{
				message.Append(" (and one more type)");
			}
			else if (additionalServicesCount > 1)
			{
				message.AppendFormat(" (and {0} more types)", additionalServicesCount);
			}
			var impl = handler.ComponentModel.Implementation;
			if (additionalServicesCount == 0 && impl == services[0])
			{
				return message.ToString();
			}
			message.Append(" / ");
			if (impl == null)
			{
				message.Append("no type");
			}
			else if (impl == typeof(LateBoundComponent))
			{
				message.Append("late bound type");
			}
			else
			{
				message.Append(impl.Name);
			}
			return message.ToString();
		}

		private IEnumerable<IComponentDebuggerExtension> GetExtensions(IHandler handler)
		{
			var handlerExtensions = handler.ComponentModel.ExtendedProperties["DebuggerExtensions"];
			return (IEnumerable<IComponentDebuggerExtension>)handlerExtensions ??
			       Enumerable.Empty<IComponentDebuggerExtension>();
		}
	}
#endif
}