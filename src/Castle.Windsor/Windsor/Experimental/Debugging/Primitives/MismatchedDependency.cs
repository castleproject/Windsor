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
	using System.Diagnostics;

	using Castle.MicroKernel;

#if !SILVERLIGHT
	public class MismatchedDependency
	{

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler[] handlers;

		public MismatchedDependency(string description, IHandler[] handlers)
		{
			this.description = description;
			this.handlers = handlers;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public IHandler[] Handlers
		{
			get { return handlers; }
		}

		public string Description
		{
			get { return description; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItem[] ViewItems
		{
			get { return Array.ConvertAll(handlers,BuildComponentView); }
		}

		private DebuggerViewItem BuildComponentView(IHandler handler)
		{
			var defaultExtension = new DefaultComponentView(handler);
			var item = new ComponentDebuggerView(handler, defaultExtension);
			return new DebuggerViewItem(handler.ComponentModel.Name, handler.ComponentModel.GetLifestyleDescription(), item);
		}
	}
#endif
}