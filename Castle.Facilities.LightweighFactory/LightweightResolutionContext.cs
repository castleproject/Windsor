namespace Castle.Facilities.LightweighFactory
{
	using System.Collections;
	using System.Collections.Generic;

	public class LightweightResolutionContext
	{
		private readonly HashSet<object> usedItems = new HashSet<object>();

		public object NextNotUsed(IEnumerable items)
		{
			foreach (var item in items)
			{
				if(!usedItems.Contains(item))
				{
					usedItems.Add(item);
					return item;
				}
			}

			return null;
		}
	}
}