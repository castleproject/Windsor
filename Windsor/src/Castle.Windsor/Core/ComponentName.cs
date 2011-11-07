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

namespace Castle.Core
{
	using System;

	using Castle.Core.Internal;

	public class ComponentName
	{
		public ComponentName(string name, bool setByUser)
		{
			Name = Must.NotBeEmpty(name, "name");
			SetByUser = setByUser;
		}

		public string Name { get; private set; }
		public bool SetByUser { get; private set; }

		public override string ToString()
		{
			return Name;
		}

		internal void SetName(string value)
		{
			Name = Must.NotBeEmpty(value, "value");
			SetByUser = true;
		}

		/// <summary>
		///   Gets the default name for component implemented by <paramref name = "componentType" /> which will be used in case when user does not provide one explicitly.
		/// </summary>
		/// <param name = "componentType"></param>
		/// <returns></returns>
		public static ComponentName DefaultFor(Type componentType)
		{
			return new ComponentName(DefaultNameFor(componentType), false);
		}

		/// <summary>
		///   Gets the default name for component implemented by <paramref name = "componentType" /> which will be used in case when user does not provide one explicitly.
		/// </summary>
		/// <param name = "componentType"></param>
		/// <returns></returns>
		public static string DefaultNameFor(Type componentType)
		{
			return componentType.FullName;
		}
	}
}