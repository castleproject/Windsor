namespace Castle.MicroKernel.Context
{
	using System.Collections;
	using System.Collections.Generic;

	public class ComponentArgumentsEnumerator : IDictionaryEnumerator
	{
		private readonly IList<IArgumentsStore> stores;
		private int currentStoreIndex;
		private IEnumerator<KeyValuePair<object, object>> currentEnumerator;

		public ComponentArgumentsEnumerator(IList<IArgumentsStore> stores)
		{
			this.stores = stores;
			currentStoreIndex = -1;
		}

		public bool MoveNext()
		{
			if (currentEnumerator != null)
			{
				var hasNext = currentEnumerator.MoveNext();
				if (hasNext == false)
				{
					currentEnumerator = null;
					return MoveNext();
				}
				return hasNext;
			}

			currentStoreIndex++;
			if (currentStoreIndex < stores.Count)
			{
				currentEnumerator = stores[currentStoreIndex].GetEnumerator();
				return MoveNext();
			}

			return false;
		}

		public void Reset()
		{
			currentEnumerator.Reset();
			currentEnumerator = null;
			currentStoreIndex = -1;
		}

		public object Current
		{
			get { return Entry; }
		}

		public object Key
		{
			get
			{
				if(currentEnumerator == null)
				{
					return null;
				}
				return currentEnumerator.Current.Key;
			}
		}

		public object Value
		{
			get
			{
				if (currentEnumerator == null)
				{
					return null;
				} 
				return currentEnumerator.Current.Value;
			}
		}

		public DictionaryEntry Entry
		{
			get { return new DictionaryEntry(Key, Value); }
		}
	}
}