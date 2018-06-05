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

namespace Castle.MicroKernel.Tests.ClassComponents
{
	public class HasTwoConstructors3
	{
		public SimpleComponent1 X { get; private set; }
		public SimpleComponent2 Y { get; private set; }
		public SimpleComponent3 A { get; private set; }

		public HasTwoConstructors3(SimpleComponent3 a)
		{
			A = a;
		}

		public HasTwoConstructors3(SimpleComponent1 x, SimpleComponent2 y)
		{
			X = x;
			Y = y;
		}
	}
}