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

namespace Castle.Facilities.Metadata
{
	using Castle.Components.DictionaryAdapter;

	public class NoOpSetter : IDictionaryPropertySetter
	{
		public int ExecutionOrder
		{
			get { return -1; }
		}

		public bool SetPropertyValue(IDictionaryAdapter dictionaryAdapter, string key, ref object value,
		                             PropertyDescriptor property)
		{
			// we ignore the property
			return false;
		}
	}
}