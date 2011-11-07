// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
	public class HasOnlyOneArgMatchingDelegatesParameter
	{
		private readonly string arg1;
		private readonly string name;

		public HasOnlyOneArgMatchingDelegatesParameter(string arg1, string name)
		{
			this.arg1 = arg1;
			this.name = name;
		}

		public string Arg1
		{
			get { return arg1; }
		}

		public string Name
		{
			get { return name; }
		}
	}
}