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

namespace Castle.Core.Internal
{
	using System;
	using System.Collections;
	using System.ComponentModel;

#if SILVERLIGHT
	using System.Linq;
#endif

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CollectionExtensions
	{
		public static TResult[] ConvertAll<T, TResult>(this T[] items, Converter<T, TResult> transformation)
		{
#if SILVERLIGHT
			return items.Select(transformation.Invoke).ToArray();
#else
			return Array.ConvertAll(items, transformation);
#endif
		}

		/// <summary>
		///   Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.
		/// </summary>
		/// <param name = "this"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this IEnumerable @this)
		{
			return @this == null || @this.GetEnumerator().MoveNext() == false;
		}
	}
}