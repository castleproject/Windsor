// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Collection of <see cref="InterceptorReference"/>
	/// </summary>
#if !SILVERLIGHT
	[Serializable]
#endif
	public class InterceptorReferenceCollection : ICollection<InterceptorReference>
	{
		private readonly LinkedList<InterceptorReference> list = new LinkedList<InterceptorReference>();
		/// <summary>
		/// Adds the specified item.
		/// </summary>
		/// <param name="item">The interceptor.</param>
		public void Add(InterceptorReference item)
		{
			list.AddLast(item);
		}

		public void Clear()
		{
			list.Clear();
		}

		public bool Contains(InterceptorReference item)
		{
			return list.Contains(item);
		}

		void ICollection<InterceptorReference>.CopyTo(InterceptorReference[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(InterceptorReference item)
		{
			list.Remove(item);
			return true;
		}

		/// <summary>
		/// Adds the specified interceptor as the first.
		/// </summary>
		/// <param name="item">The interceptor.</param>
		public void AddFirst(InterceptorReference item)
		{
			list.AddFirst(item);
		}

		/// <summary>
		/// Adds the specified interceptor as the last.
		/// </summary>
		/// <param name="item">The interceptor.</param>
		public void AddLast(InterceptorReference item)
		{
			list.AddLast(item);
		}

		/// <summary>
		/// Inserts the specified interceptor at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="item">The interceptor.</param>
		public void Insert(int index, InterceptorReference item)
		{
			if(index==0)
			{
				AddFirst(item);
				return;
			}
			if(index == list.Count)
			{
				AddLast(item);
				return;
			}
			var previous = list.First;
			for (int i = 1; i < index; i++)
			{
				previous = previous.Next;
			}
			list.AddAfter(previous, item);
		}

		/// <summary>
		/// Gets a value indicating whether this instance has interceptors.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has interceptors; otherwise, <c>false</c>.
		/// </value>
		public bool HasInterceptors
		{
			get { return Count != 0; }
		}

		/// <summary>
		/// Gets the number of
		/// elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <value></value>
		public int Count
		{
			get { return list.Count; }
		}

		bool ICollection<InterceptorReference>.IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Returns an enumerator that can iterate through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/>
		/// that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		/// <summary>
		/// Adds the interceptor to the end of the interceptors list if it does not exist already.
		/// </summary>
		/// <param name="interceptorReference">The interceptor reference.</param>
		public void AddIfNotInCollection(InterceptorReference interceptorReference)
		{
			if (list.Contains(interceptorReference) == false)
				list.AddLast(interceptorReference);
		}

		IEnumerator<InterceptorReference> IEnumerable<InterceptorReference>.GetEnumerator()
		{
		    foreach (var reference in list)
		    {
		        yield return reference;
		    }
		}
	}
}