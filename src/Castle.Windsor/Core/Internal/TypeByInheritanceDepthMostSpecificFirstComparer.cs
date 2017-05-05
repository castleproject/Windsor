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
	using System.Collections.Generic;

	public class TypeByInheritanceDepthMostSpecificFirstComparer : IComparer<Type>
	{
		// TODO: make sure generics (open?) are also handled
		public int Compare(Type x, Type y)
		{
			if (x == y)
			{
				return 0;
			}
			if (x.IsAssignableFrom(y))
			{
				return 1;
			}
			if (y.IsAssignableFrom(x))
			{
				return -1;
			}
			var message =
				String.Format("Types {0} and {1} are unrelated. That is not allowed. Are you sure you want to make them both services on the same component?",
				              x, y);
			throw new ArgumentOutOfRangeException("x", message);
		}
	}
}