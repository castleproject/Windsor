#region license

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

#endregion

namespace Castle.Facilities.Transactions.IO
{
	using System.Diagnostics.Contracts;

	public static class PathEx
	{
		public static PathInfo ToPathInfo(this string input)
		{
			Contract.Requires(input != null);
			return PathInfo.Parse(input);
		}

		/// <summary>
		/// 	Combines an input path and a path together
		/// 	using System.IO.Path.Combine and returns the result.
		/// </summary>
		public static string Combine(this string input, string path)
		{
			Contract.Requires(input != null);
			Contract.Requires(path != null);
			return System.IO.Path.Combine(input, path);
		}
	}
}