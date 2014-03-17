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
	///     Collection of <see cref = "ConstructorCandidate" />
	/// </summary>
	[Serializable]
	public class ConstructorCandidateCollection : IMutableCollection<ConstructorCandidate>
	{
		private readonly SimpleSortedSet<ConstructorCandidate> ctors = new SimpleSortedSet<ConstructorCandidate>();

		public int Count
		{
			get { return ctors.Count; }
		}

		public ConstructorCandidate this[int index]
		{
			get { return ctors[index]; }
		}

		[DebuggerStepThrough]
		public IEnumerator<ConstructorCandidate> GetEnumerator()
		{
			return ctors.GetEnumerator();
		}

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void IMutableCollection<ConstructorCandidate>.Add(ConstructorCandidate item)
		{
			ctors.Add(item);
		}

		bool IMutableCollection<ConstructorCandidate>.Remove(ConstructorCandidate item)
		{
			return ctors.Remove(item);
		}
	}
}