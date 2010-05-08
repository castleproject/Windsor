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

namespace Castle.Facilities.TypedFactory
{
	using System;

	public class FactoryParameter
	{
		private readonly Type type;
		private readonly string name;
		private readonly int position;
		private readonly object value;
		private bool used;

		public FactoryParameter(Type type, string name,int position, object value)
		{
			this.type = type;
			this.name = name;
			this.position = position;
			this.value = value;
		}

		public int Position
		{
			get { return position; }
		}

		public bool Used
		{
			get { return used; }
		}

		public Type Type
		{
			get { return type; }
		}

		public string Name
		{
			get { return name; }
		}

		public object ResolveValue()
		{
			used = true;
			return value;
		}
	}
}