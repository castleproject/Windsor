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
	[DebuggerDisplay("{key} {description,nq}")]
	public class ComponentDebuggerView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IComponentDebuggerExtension[] extension;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler handler;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string key;

		public ComponentDebuggerView(IHandler handler, params IComponentDebuggerExtension[] defaultExtension)
		{
			key = handler.ComponentModel.Name;
			this.handler = handler;
			extension = defaultExtension.Concat(GetExtensions(handler)).ToArray();
			description = handler.GetServicesDescription();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItem[] Extensions
		{
			get { return extension.SelectMany(e => e.Attach()).ToArray(); }
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