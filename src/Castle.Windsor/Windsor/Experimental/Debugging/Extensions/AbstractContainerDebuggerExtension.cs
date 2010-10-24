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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.Windsor.Experimental.Debugging.Primitives;
	
#if !SILVERLIGHT
	public abstract class AbstractContainerDebuggerExtension : IContainerDebuggerExtension
	{
		public abstract IEnumerable<DebuggerViewItem> Attach();

		public abstract void Init(IKernel kernel);

		protected ComponentDebuggerView DefaultComponentView(MetaComponent component)
		{
			return new ComponentDebuggerView(component,
			                                 new DefaultComponentView(component.Handler, component.ForwardedTypes));
		}

		protected IEnumerable<MetaComponent> GetMetaComponents(
			IDictionary<string, IHandler> flatKeyHandlers)
		{
			var lookup = new Dictionary<IHandler, KeyValuePair<string, IList<Type>>>();
			foreach (var handler in flatKeyHandlers)
			{
				var actual = handler.Value;
				var forwarding = handler.Value as ForwardingHandler;
				if (forwarding != null)
				{
					actual = forwarding.Target;
				}
				KeyValuePair<string, IList<Type>> list;
				if (lookup.TryGetValue(actual, out list) == false)
				{
					list = new KeyValuePair<string, IList<Type>>(handler.Key, new List<Type>(4));
					lookup.Add(actual, list);
				}
				if (forwarding != null)
				{
					list.Value.Add(forwarding.Service);
				}
			}
			return lookup.Select(c => new MetaComponent(c.Value.Key, c.Key, c.Value.Value));
		}
	}
#endif
}