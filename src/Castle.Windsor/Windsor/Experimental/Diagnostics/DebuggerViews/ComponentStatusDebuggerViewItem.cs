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
	using System.Text;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;

#if !SILVERLIGHT
	public class ComponentStatusDebuggerViewItem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IExposeDependencyInfo handler;

		public ComponentStatusDebuggerViewItem(IExposeDependencyInfo handler)
		{
			this.handler = handler;
		}

		public string Message
		{
			get
			{
				var message = new StringBuilder("Some dependencies of this component could not be statically resolved.");
				if (handler == null)
				{
					return message.ToString();
				}
				var inspector = new DependencyInspector(message);
				handler.ObtainDependencyDetails(inspector);

				return inspector.Message;
			}
		}

		public override string ToString()
		{
			return "This component may not resolve properly.";
		}
	}
#endif
}