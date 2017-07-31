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

#if NET45

namespace Castle.Facilities.Owin.WebHost.Lifestyles
{
	using System;
	using System.Web;

	using Castle.Core.Internal;

	internal class MsSystemWebHttpContextScopeWrapper
	{
		private static readonly Lock @lock = Lock.Create();

		public static T GetOrSet<T>(string key, Func<T> create)
		{
			var result = Get<T>(key);
			if (result != null) 
				return result;

			using (@lock.ForWriting())
			{
				result = Get<T>(key);
				if (result != null)
					return result;

				result = create();
				HttpContext.Current.Items[key] = result;
			}

			return result;
		}

		public static T Get<T>(string key)
		{
			using (@lock.ForReading())
			{
				return (T) HttpContext.Current.Items[key];
			}
		}
	}
}

#endif
