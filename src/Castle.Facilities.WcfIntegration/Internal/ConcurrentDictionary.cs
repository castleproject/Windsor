namespace Castle.Facilities.WcfIntegration.Internal
{
#if DOTNET35
	using System;
	using System.Collections.Generic;

	public class ConcurrentDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		private readonly static object sync = new object();

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
		{
			lock (sync)
			{
				if (!ContainsKey(key))
				{
					Add(key, factory(key));
				}

				return this[key];
			}
		}
	}
#endif
}
