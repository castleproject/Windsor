// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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

	using Castle.Core.Internal;

	/// <summary>
	///     Collection of <see cref = "DependencyModel" />.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("Count = {dependencies.Count}")]
	public class DependencyModelCollection : IMutableCollection<DependencyModel>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private readonly List<DependencyModel> dependencies = new List<DependencyModel>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public int Count
		{
			get { return dependencies.Count; }
		}

		[DebuggerStepThrough]
		public IEnumerator<DependencyModel> GetEnumerator()
		{
			return dependencies.GetEnumerator();
		}

		public void Add(DependencyModel dependencyModel)
		{
			if (dependencyModel == null)
			{
				throw new ArgumentNullException(nameof(dependencyModel));
			}
			dependencies.Add(dependencyModel);
		}

		public bool Remove(DependencyModel dependencyModel)
		{
			return dependencies.Remove(dependencyModel);
		}

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}