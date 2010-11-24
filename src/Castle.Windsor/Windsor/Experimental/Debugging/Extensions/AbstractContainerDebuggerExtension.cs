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

namespace Castle.Windsor.Experimental.Debugging.Extensions
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.Windsor.Experimental.Debugging.Primitives;
	
#if !SILVERLIGHT
	public abstract class AbstractContainerDebuggerExtension : IContainerDebuggerExtension
	{
		public abstract IEnumerable<DebuggerViewItem> Attach();

		public abstract void Init(IKernel kernel);

		protected ComponentDebuggerView DefaultComponentView(MetaComponent component)
		{
			return new ComponentDebuggerView(component, new DefaultComponentView(component.Handler));
		}

		protected IEnumerable<MetaComponent> GetMetaComponents(IDictionary<string, IHandler> flatKeyHandlers)
		{
			return flatKeyHandlers.Select(c => new MetaComponent(c.Key, c.Value));
		}
	}
#endif
}