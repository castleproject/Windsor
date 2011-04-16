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

namespace Castle.Core.Internal
{
	using System.Threading;

	public struct ThreadSafeFlag
	{
		/// <summary>
		///   0 == false, 1 = =true
		/// </summary>
		private int signaled;

		/// <summary>
		///   Signals (sets) the flag.
		/// </summary>
		/// <returns><c>true</c> if the current thread signaled the flag, <c>false</c> if some other thread signaled the flag before.</returns>
		public bool Signal()
		{
			return Interlocked.Exchange(ref signaled, 1) == 0;
		}
	}
}