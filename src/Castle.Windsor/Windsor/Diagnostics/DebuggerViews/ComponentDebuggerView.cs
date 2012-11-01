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

namespace Castle.Windsor.Diagnostics.DebuggerViews
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.Helpers;

#if !SILVERLIGHT
	[DebuggerDisplay("{description,nq}", Name = "{name,nq}")]
	public class ComponentDebuggerView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IComponentDebuggerExtension[] extension;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string name;

		public ComponentDebuggerView(IHandler handler, string description, params IComponentDebuggerExtension[] defaultExtension)
		{
			name = handler.GetComponentName();
			this.description = description;
			extension = defaultExtension.Concat(GetExtensions(handler)).ToArray();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string Description
		{
			get { return description; }
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

		public static ComponentDebuggerView BuildFor(IHandler handler, string description = null)
		{
			var extensions = new List<IComponentDebuggerExtension> { new DefaultComponentViewBuilder(handler) };
			extensions.AddRange(GetExtensions(handler));
			return BuildRawFor(handler, description ?? handler.ComponentModel.GetLifestyleDescription(), extensions.ToArray());
		}

		public static ComponentDebuggerView BuildRawFor(IHandler handler, string description, IComponentDebuggerExtension[] extensions)
		{
			return new ComponentDebuggerView(handler, description, extensions);
		}

		public static ComponentDebuggerView BuildRawFor(IHandler handler, string description, DebuggerViewItem[] items)
		{
			return new ComponentDebuggerView(handler, description, new ComponentDebuggerExtension(items));
		}

		private static IEnumerable<IComponentDebuggerExtension> GetExtensions(IHandler handler)
		{
			var handlerExtensions = handler.ComponentModel.ExtendedProperties["DebuggerExtensions"];
			return (IEnumerable<IComponentDebuggerExtension>)handlerExtensions ??
			       Enumerable.Empty<IComponentDebuggerExtension>();
		}
	}
#endif
}