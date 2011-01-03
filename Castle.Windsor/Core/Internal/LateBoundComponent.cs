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
	using System.Diagnostics;

	/// <summary>
	///   Marker class used to denote components that have late bound type
	///   That is the actual type is not known exactly at the time when <see cref = "ComponentModel" />
	///   is created. Those are for example components instantiated via abstract factory.
	/// </summary>
	public sealed class LateBoundComponent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static readonly object Instance = new LateBoundComponent();

		public override string ToString()
		{
			return "Late bound component, actual type is not known statically.";
		}
	}
}