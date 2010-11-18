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

namespace Castle.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	///   Collection of <see cref = "DependencyModel" />.
	/// </summary>
	[Serializable]
	public class DependencyModelCollection : IEnumerable<DependencyModel>
	{
		private readonly List<DependencyModel> dependencies = new List<DependencyModel>(8);

		public DependencyModelCollection()
		{
		}

		public bool HasDependencies
		{
			get { return dependencies.Count > 0; }
		}

		public void Add(DependencyModel dependencyModel)
		{
			if (dependencyModel == null)
			{
				throw new ArgumentNullException("dependencyModel");
			}
			dependencies.Add(dependencyModel);
		}

		public void AddRange(DependencyModelCollection other)
		{
			if (other == null || other.dependencies.Count == 0)
			{
				return;
			}
			dependencies.AddRange(other.dependencies);
		}

		public void Clear()
		{
			dependencies.Clear();
		}

		public bool Contains(DependencyModel dependencyModel)
		{
			return dependencies.Contains(dependencyModel);
		}

		public bool Remove(DependencyModel dependencyModel)
		{
			return dependencies.Remove(dependencyModel);
		}

		public IEnumerator<DependencyModel> GetEnumerator()
		{
			return dependencies.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}