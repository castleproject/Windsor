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

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Delegates
{
	using System;

	public class UsesTwoFooDelegates
	{
		private readonly Func<int, Foo> one;
		private readonly Func<int, Foo> two;
		private int counter;

		public UsesTwoFooDelegates(Func<int, Foo> one, Func<int, Foo> two)
		{
			this.one = one;
			this.two = two;
			counter = 0;
		}

		public Func<int, Foo> One
		{
			get { return one; }
		}

		public Func<int, Foo> Two
		{
			get { return two; }
		}

		public Foo GetFooOne()
		{
			return one(++counter);
		}

		public Foo GetFooTwo()
		{
			return two(++counter);
		}
	}
}