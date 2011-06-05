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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using Castle.IO.Internal;

namespace Castle.IO
{
	///<summary>
	///	Utility class meant to replace the <see cref = "System.IO.Path" /> class completely. This class handles these types of paths:
	///	<list>
	///		<item>UNC network paths: \\server\folder</item>
	///		<item>UNC-specified network paths: \\?\UNC\server\folder</item>
	///		<item>IPv4 network paths: \\192.168.3.22\folder</item>
	///		<item>IPv4 network paths: \\[2001:34:23:55::34]\folder</item>
	///		<item>Rooted paths: /dev/cdrom0</item>
	///		<item>Rooted paths: C:\folder</item>
	///		<item>UNC-rooted paths: \\?\C:\folder\file</item>
	///		<item>Fully expanded IPv6 paths</item>
	///	</list>
	///</summary>
	public partial class Path
	{
		// can of worms shut!

		// TODO: v3.2: 2001:0db8::1428:57ab and 2001:0db8:0:0::1428:57ab are not matched!
		// ip6: thanks to http://blogs.msdn.com/mpoulson/archive/2005/01/10/350037.aspx

		private static readonly List<char> _InvalidChars = new List<char>(GetInvalidPathChars());
		//			_Reserved = new List<string>("CON|PRN|AUX|NUL|COM1|COM2|COM3|COM4|COM5|COM6|COM7|COM8|COM9|LPT1|LPT2|LPT3|LPT4|LPT5|LPT6|LPT7|LPT8|LPT9"
		//				.Split('|'));

		///<summary>
		///	Returns whether the path is rooted. An empty string isn't.
		///</summary>
		///<param name = "path">Gets whether the path is rooted or relative.</param>
		///<returns>Whether the path is rooted or not.</returns>
		///<exception cref = "ArgumentNullException">If the passed argument is null.</exception>
		[Pure]
		public static bool IsPathRooted(string path)
		{
			Contract.Requires(path != null);
			if (string.IsNullOrEmpty(path)) return false;
			return !string.IsNullOrEmpty(PathInfo.Parse(path).Root);
		}

		/// <summary>
		/// 	Gets the path root, i.e. e.g. \\?\C:\ if the passed argument is \\?\C:\a\b\c.abc.
		/// </summary>
		/// <param name = "path">The path to get the root for.</param>
		/// <returns>The string denoting the root.</returns>
		[Pure]
		public static string GetPathRoot(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			if (ContainsInvalidChars(path)) throw new ArgumentException("path contains invalid characters.");
			return PathInfo.Parse(path).Root;
		}

		[Pure]
		private static bool ContainsInvalidChars(string path)
		{
			var c = _InvalidChars.Count;
			var l = path.Length;

			for (var i = 0; i < l; i++)
				for (var j = 0; j < c; j++)
					if (path[i] == _InvalidChars[j])
						return true;
			return false;
		}

		///<summary>
		///	Gets a path without root.
		///</summary>
		///<param name = "path"></param>
		///<returns></returns>
		///<exception cref = "ArgumentNullException"></exception>
		[Pure]
		public static string GetPathWithoutRoot(string path)
		{
			Contract.Requires(path != null);

			if (path.Length == 0) return string.Empty;
			var startIndex = GetPathRoot(path).Length;
			if (path.Length < startIndex) return string.Empty;
			return path.Substring(startIndex);
		}

		///<summary>
		///	Normalize all the directory separation chars.
		///	Also removes empty space in beginning and end of string.
		///</summary>
		///<param name = "pathWithAlternatingChars"></param>
		///<returns>The directory string path with all occurrances of the alternating chars
		///	replaced for that specified in <see cref = "System.IO.Path.DirectorySeparatorChar" /></returns>
		[Pure]
		public static string NormDirSepChars(string pathWithAlternatingChars)
		{
			Contract.Requires(!string.IsNullOrEmpty(pathWithAlternatingChars));
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			var sb = new StringBuilder();

			for (var i = 0; i < pathWithAlternatingChars.Length; i++)
				if (pathWithAlternatingChars[i] == '\\' || pathWithAlternatingChars[i] == '/')
					sb.Append(DirectorySeparatorChar);
				else
					sb.Append(pathWithAlternatingChars[i]);

			var ret = sb.ToString().Trim(' ');

			Contract.Assume(!string.IsNullOrEmpty(ret),
			                "because input was non-empty string and for every item in the string "
			                + "we append to the string builder a non-whitespace char, so it can't be empty");

			return ret;
		}

		///<summary>
		///	Gets path info (drive and non root path)
		///</summary>
		///<param name = "path">The path to get the info from.</param>
		///<returns></returns>
		///<exception cref = "ArgumentNullException"></exception>
		[Pure]
		public static PathInfo GetPathInfo(string path)
		{
			Contract.Requires(path != null);
			return PathInfo.Parse(path);
		}

		///<summary>
		///	Gets the full path for a given path.
		///</summary>
		///<param name = "path"></param>
		///<returns>The full path string</returns>
		///<exception cref = "ArgumentNullException">if path is null</exception>
		[Pure]
		public static string GetFullPath(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			Contract.Ensures(Contract.Result<string>() != null);

			if (path.StartsWith("\\\\?\\") || path.StartsWith("\\\\.\\")) return System.IO.Path.GetFullPath(path.Substring(4));
			if (path.StartsWith("\\\\?\\UNC\\")) return System.IO.Path.GetFullPath(path.Substring(8));
			if (path.StartsWith("file:///")) return new Uri(path).LocalPath;
			// TODO: Replace with Win32-call
			return System.IO.Path.GetFullPath(path);
		}

		/// <summary>
		/// 	Removes the last directory/file off the path.
		/// 
		/// 	For a path "/a/b/c" would return "/a/b"
		/// 	or for "\\?\C:\folderA\folder\B\C\d.txt" would return "\\?\C:\folderA\folder\B\C"
		/// </summary>
		/// <param name = "path">The path string to modify</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength",
			Justification = "It would change the semantics.")]
		[Pure]
		public static string GetPathWithoutLastBit(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			Contract.Requires(path.Length >= 2);

			var chars = new List<char>
			            	{
			            		DirectorySeparatorChar,
			            		AltDirectorySeparatorChar
			            	};

			var endsWithSlash = false;
			var secondLast = -1;
			var last = -1;
			var lastType = chars[0];

			for (var i = 0; i < path.Length; i++)
			{
				if (i == path.Length - 1 && chars.Contains(path[i]))
					endsWithSlash = true;

				if (!chars.Contains(path[i]))
					continue;

				secondLast = last;
				last = i;
				lastType = path[i];
			}

			if (last < 0)
				throw new ArgumentException(string.Format("Could not find a path separator character in the path \"{0}\"", path));

			var length = endsWithSlash ? (secondLast == -1 ? 0 : secondLast) : last;
			var res = path.Substring(0, length);
			return res == string.Empty ? new string(lastType, 1) : res;
		}

		[Pure]
		public static string GetFileName(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path), "path musn't be null");

			if (path.EndsWith("/") || path.EndsWith("\\"))
				return string.Empty;

			var nonRoot = PathInfo.Parse(path).FolderAndFiles;

			int strIndex;

			// resharper is wrong that you can transform this to a ternary operator.
			if ((strIndex = nonRoot.LastIndexOfAny(new[] {DirectorySeparatorChar, AltDirectorySeparatorChar})) != -1)
				return nonRoot.Substring(strIndex + 1);

			return nonRoot;
		}


		[Pure]
		public static string GetDirectoryName(string path)
		{
			return GetFileName(path);
		}

		[Pure]
		public static bool HasExtension(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			return GetFileName(path).Length != GetFileNameWithoutExtension(path).Length;
		}

		[Pure]
		public static string GetExtension(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			var fn = GetFileName(path);
			var lastPeriod = fn.LastIndexOf('.');
			return lastPeriod == -1 ? string.Empty : fn.Substring(lastPeriod + 1);
		}

		[Pure]
		public static string GetFileNameWithoutExtension(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

			var filename = GetFileName(path);
			var lastPeriod = filename.LastIndexOf('.');

			return lastPeriod == -1 ? filename : filename.Substring(0, lastPeriod);
		}

		[Pure]
		public static string GetRandomFileName()
		{
			return System.IO.Path.GetRandomFileName();
		}

		[Pure]
		public static char[] GetInvalidPathChars()
		{
			return System.IO.Path.GetInvalidPathChars();
		}

		[Pure]
		public static char[] GetInvalidFileNameChars()
		{
			return System.IO.Path.GetInvalidFileNameChars();
		}

		[Pure]
		public static char DirectorySeparatorChar
		{
			get { return System.IO.Path.DirectorySeparatorChar; }
		}

		[Pure]
		public static char AltDirectorySeparatorChar
		{
			get { return System.IO.Path.AltDirectorySeparatorChar; }
		}

		[Pure]
		public static char[] GetDirectorySeparatorChars()
		{
			return new[] {DirectorySeparatorChar, AltDirectorySeparatorChar};
		}
	}
}