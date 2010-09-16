namespace Castle.Windsor.Experimental.Debugging.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.Windsor.Experimental.Debugging.Primitives;

	public abstract class AbstractContainerDebuggerExtension : IContainerDebuggerExtension
	{
		protected IDictionary<IHandler, KeyValuePair<string, IList<Type>>> GetKeyToHandlersLookup(IDictionary<string, IHandler> flatKeyHandlers)
		{
			var lookup = new Dictionary<IHandler, KeyValuePair<string, IList<Type>>>();
			foreach (var handler in flatKeyHandlers)
			{
				var actual = handler.Value;
				var forwarding = handler.Value as ForwardingHandler;
				if(forwarding!=null)
				{
					actual = forwarding.Target;
				}
				KeyValuePair<string, IList<Type>> list;
				if(lookup.TryGetValue(actual, out list) == false)
				{
					list = new KeyValuePair<string, IList<Type>>(handler.Key, new List<Type>(4));
					lookup.Add(actual, list);
				}
				if(forwarding!=null)
				{
					list.Value.Add(forwarding.Service);
				}
			}
			return lookup;
		}

		public abstract IEnumerable<DebuggerViewItem> Attach();

		public abstract void Init(IKernel kernel);

		protected ComponentDebuggerView DefaultComponentView(KeyValuePair<IHandler, KeyValuePair<string, IList<Type>>> handler)
		{
			return new ComponentDebuggerView(handler.Key,
			                                 handler.Value,
			                                 new DefaultComponentView(handler.Key, handler.Value.Value.ToArray()));
		}
	}
}