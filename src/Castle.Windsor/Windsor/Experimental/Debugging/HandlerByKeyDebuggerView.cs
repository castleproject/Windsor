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

namespace Castle.Windsor.Experimental.Debugging
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core.Internal;
	using Castle.MicroKernel;

	[DebuggerDisplay("{key} / [{ServiceString}]")]
	public class HandlerByKeyDebuggerView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IComponentDebuggerExtension[] extension;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler service;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string key;

		public HandlerByKeyDebuggerView(string key, IHandler service, params IComponentDebuggerExtension[] defaultExtension)
		{
			this.key = key;
			this.service = service;
			extension = defaultExtension.Concat(GetExtensions(service)).ToArray();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItem[] Extensions
		{
			get { return extension.SelectMany(e => e.Attach()).ToArray(); }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string ServiceString
		{
			get
			{
				var value = service.Service.Name;
				var impl = service.ComponentModel.Implementation;
				if (impl == service.Service)
				{
					return value;
				}
				value += " / ";
				if (impl == null)
				{
					value += "no type";
				}
				else if (impl == typeof(LateBoundComponent))
				{
					value += "late bound type";
				}
				else
				{
					value += impl.Name;
				}
				return value;
			}
		}

		private IEnumerable<IComponentDebuggerExtension> GetExtensions(IHandler handler)
		{
			var handlerExtensions = handler.ComponentModel.ExtendedProperties["DebuggerExtensions"];
			return (IEnumerable<IComponentDebuggerExtension>)handlerExtensions ??
			       Enumerable.Empty<IComponentDebuggerExtension>();
		}
	}
}