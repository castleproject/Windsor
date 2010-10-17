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
#if !SILVERLIGHT
	[Serializable]
#endif
	public class DependencyModelCollection : IEnumerable<DependencyModel>
	{
		private readonly ICollection<DependencyModel> dependencies =
#if DOTNET35 || SL3
			new List<DependencyModel>();
#else
			new HashSet<DependencyModel>();
#endif

		public void Add(DependencyModel dependencyModel)
		{
			if (dependencyModel == null)
			{
				throw new ArgumentNullException("dependencyModel");
			}
#if DOTNET35 || SL3
			if(dependencies.Contains(dependencyModel))
			{
				return;
			}
#endif
			dependencies.Add(dependencyModel);
		}

		public void AddRange(DependencyModelCollection dependencies)
		{
			if (dependencies == null)
			{
				return;
			}

			foreach (var model in dependencies)
			{
				if (model == null)
				{
					throw new ArgumentNullException("dependencies", "item in the collection is null");
				}

				Add(model);
			}
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