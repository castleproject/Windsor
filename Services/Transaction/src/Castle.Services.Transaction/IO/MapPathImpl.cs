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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction.IO
{
	///<summary>
	///	An implementation of the MapPath which seems to be working well with
	///	both testfixtures and online. Consumed by <see cref = "IDirectoryAdapter" />
	///	(or any other object wanting the functionality).
	///</summary>
	public class MapPathImpl : IMapPath
	{
		private readonly Func<string, string> _Function;

		///<summary>
		///	Default c'tor.
		///</summary>
		public MapPathImpl()
		{
		}

		/// <summary>
		/// 	Function may be null.
		/// </summary>
		/// <param name = "function"></param>
		public MapPathImpl(Func<string, string> function)
		{
			_Function = function;
		}

		///<summary>
		///	Gets the absolute path given a string formatted
		///	as a map path, for example:
		///	"~/plugins" or "plugins/integrated" or "C:\a\b\c.txt" or "\\?\C:\a\b"
		///	would all be valid map paths.
		///</summary>
		///<param name = "path"></param>
		///<returns></returns>
		[SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength",
			Justification = "I don't care that much about performance")]
		public string MapPath(string path)
		{
			if (Path.IsRooted(path))
				return Path.GetFullPath(path);

			if (_Function != null)
			{
				var mapPath = _Function(path);
				Contract.Assume(!string.IsNullOrEmpty(mapPath),
				                "This is a user-provided function, and code-contracts don't allow me to reason with the typed for Func in a Requires in c'tor.");
				return mapPath;
			}

			path = Path.NormDirSepChars(path);

			if (string.IsNullOrEmpty(path))
				return AppDomain.CurrentDomain.BaseDirectory;

			if (path[0] == '~')
				path = path.Substring(1);

			if (path.Length == 0)
				return AppDomain.CurrentDomain.BaseDirectory;

			if (Path.DirectorySeparatorChar == path[0])
				path = path.Substring(1);

			return path == string.Empty
			       	? AppDomain.CurrentDomain.BaseDirectory
			       	: Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory.Combine(path));
		}
	}
}