// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
	using System.Collections.Generic;

	internal static class EnumerableExtensions
	{
		public static TResult[] ConvertAll<T, TResult>(this T[] items, Func<T, TResult> converter)
		{
			var count = items.Length;
			var results = new TResult[count];

			for (int i = 0; i < count; i++)
			{
				results[i] = converter(items[i]);
			}

			return results;
		}

		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (var item in items)
			{
				action(item);
			}
		}
	}
}