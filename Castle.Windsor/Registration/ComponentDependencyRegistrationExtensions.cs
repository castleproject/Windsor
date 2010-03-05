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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections;

	public static class ComponentDependencyRegistrationExtensions
	{
		/// <summary>
		/// Inserts a new named argument with given key. If an argument for this name already exists, it will be overwritten.
		/// </summary>
		public static IDictionary Insert(this IDictionary arguments, string key, object value)
		{
			arguments[key] = value;
			return arguments;
		}

		/// <summary>
		/// Inserts a new typed argument with given type. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public static IDictionary Insert(this IDictionary arguments, Type key, object value)
		{
			arguments[key] = value;
			return arguments;
		}

		/// <summary>
		/// Inserts a new typed argument with given type. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public static IDictionary Insert<TKeyType>(this IDictionary arguments, TKeyType value)
		{
			arguments[typeof(TKeyType)] = value;
			return arguments;
		}
	}
}