#region license

// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace Castle.Services.Transaction.Tests
{
	using Castle.Services.Transaction.IO;

	public static class Exts
	{
		/// <summary>
		/// 	Combines an input path and a path together
		/// 	using System.IO.Path.Combine and returns the result.
		/// </summary>
		public static string Combine(this string input, params string[] paths)
		{
			return Path.Combine(input, paths);
		}

		/// <summary>
		/// 	Combines two paths and makes sure the 
		/// 	DIRECTORY resulting from the combination exists
		/// 	by creating it with default permissions if it doesn't.
		/// </summary>
		/// <param name = "input">The path to combine the latter with.</param>
		/// <param name = "path">The latter path.</param>
		/// <returns>The combined path string.</returns>
		public static string CombineAssert(this string input, string path)
		{
			var p = input.Combine(path);

			if (!Directory.Exists(p))
				Directory.CreateDirectory(p);

			return p;
		}
	}
}