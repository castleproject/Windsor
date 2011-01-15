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

namespace Castle.Windsor.Experimental.Diagnostics.DebuggerViews
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.Windsor.Experimental.Diagnostics.Helpers;

#if !SILVERLIGHT
	[DebuggerDisplay("{description,nq}", Name = "{name,nq}")]
	public class ComponentDebuggerView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IComponentDebuggerExtension[] extension;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler handler;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string name;

		public ComponentDebuggerView(IHandler handler, string description, params IComponentDebuggerExtension[] defaultExtension)
		{
			var componentName = handler.ComponentModel.ComponentName;
			if (componentName.SetByUser)
			{
				name = string.Format("\"{0}\" {1}", componentName.Name, handler.GetServicesDescription());
			}
			else
			{
				name = handler.GetServicesDescription();
			}
			this.handler = handler;
			this.description = description;
			extension = defaultExtension.Concat(GetExtensions(handler)).ToArray();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItem[] Extensions
		{
			get { return extension.SelectMany(e => e.Attach()).ToArray(); }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string Name
		{
			get { return name; }
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