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
	///   Collection of <see cref = "ConstructorCandidate" />
	/// </summary>
	[Serializable]
	public class ConstructorCandidateCollection : IEnumerable<ConstructorCandidate>
	{
		private readonly IList<ConstructorCandidate> ctors = new List<ConstructorCandidate>(4);
		private ConstructorCandidate fewerArgumentsCandidate;
		private bool hasAmbiguousFewerArgumentsCandidate;

		public int Count
		{
			get { return ctors.Count; }
		}

		/// <summary>
		///   Gets the fewer arguments candidate.
		/// </summary>
		/// <value>The fewer arguments candidate.</value>
		public ConstructorCandidate FewerArgumentsCandidate
		{
			get { return fewerArgumentsCandidate; }
		}

		public bool HasAmbiguousFewerArgumentsCandidate
		{
			get { return hasAmbiguousFewerArgumentsCandidate; }
		}

		public void Add(ConstructorCandidate item)
		{
			if (fewerArgumentsCandidate == null)
			{
				fewerArgumentsCandidate = item;
				hasAmbiguousFewerArgumentsCandidate = false;
			}
			else
			{
				var constructorParamCount = item.Constructor.GetParameters().Length;
				var fewerArgumentsCount = fewerArgumentsCandidate.Constructor.GetParameters().Length;

				if (constructorParamCount < fewerArgumentsCount)
				{
					fewerArgumentsCandidate = item;
				}
				else if (constructorParamCount == fewerArgumentsCount)
				{
					hasAmbiguousFewerArgumentsCandidate = true;
				}
			}
			ctors.Add(item);
		}

		public IEnumerator<ConstructorCandidate> GetEnumerator()
		{
			return ctors.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Clear()
		{
			ctors.Clear();
		}
	}
}