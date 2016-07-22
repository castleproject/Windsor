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
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;

	using Castle.Core.Configuration;

	/// <summary>
	///   Collection of <see cref = "ParameterModel" />
	/// </summary>
	[Serializable]
	[DebuggerDisplay("Count = {dictionary.Count}")]
	public class ParameterModelCollection : IEnumerable<ParameterModel>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private readonly IDictionary<string, ParameterModel> dictionary =
			new Dictionary<string, ParameterModel>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		///   Gets the count.
		/// </summary>
		/// <value>The count.</value>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public int Count
		{
			get { return dictionary.Count; }
		}

		/// <summary>
		///   Gets the <see cref = "ParameterModel" /> with the specified key.
		/// </summary>
		/// <value></value>
		public ParameterModel this[string key]
		{
			get
			{
				ParameterModel result;
				dictionary.TryGetValue(key, out result);
				return result;
			}
		}

		/// <summary>
		///   Adds the specified name.
		/// </summary>
		/// <param name = "name">The name.</param>
		/// <param name = "value">The value.</param>
		public void Add(string name, string value)
		{
			Add(name, new ParameterModel(name, value));
		}

		/// <summary>
		///   Adds the specified name.
		/// </summary>
		/// <param name = "name">The name.</param>
		/// <param name = "configNode">The config node.</param>
		public void Add(string name, IConfiguration configNode)
		{
			Add(name, new ParameterModel(name, configNode));
		}

		/// <summary>
		///   Adds the specified key.
		/// </summary>
		/// <remarks>
		///   Not implemented
		/// </remarks>
		/// <param name = "key">The key.</param>
		/// <param name = "value">The value.</param>
		private void Add(string key, ParameterModel value)
		{
			try
			{
				dictionary.Add(key, value);
			}
			catch (ArgumentException e)
			{
				throw new ArgumentException(string.Format("Parameter '{0}' already exists.", key), e);
			}
		}

		/// <summary>
		///   Returns an enumerator that can iterate through a collection.
		/// </summary>
		/// <returns>
		///   An <see cref = "T:System.Collections.IEnumerator" />
		///   that can be used to iterate through the collection.
		/// </returns>
		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return dictionary.Values.GetEnumerator();
		}

		[DebuggerStepThrough]
		IEnumerator<ParameterModel> IEnumerable<ParameterModel>.GetEnumerator()
		{
			return dictionary.Values.GetEnumerator();
		}
	}
}