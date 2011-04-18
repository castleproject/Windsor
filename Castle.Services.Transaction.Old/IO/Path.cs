#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

namespace Castle.Services.Transaction.IO
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	///<summary>
	/// Utility class meant to replace the <see cref="System.IO.Path"/> class completely. This class handles these types of paths:
	/// <list>
	/// <item>UNC network paths: \\server\folder</item>
	/// <item>UNC-specified network paths: \\?\UNC\server\folder</item>
	/// <item>IPv4 network paths: \\192.168.3.22\folder</item>
	/// <item>Rooted paths: /dev/cdrom0</item>
	/// <item>Rooted paths: C:\folder</item>
	/// <item>UNC-rooted paths: \\?\C:\folder\file</item>
	/// <item>Fully expanded IPv6 paths</item>
	/// </list>
	///</summary>
	public static class Path
	{
		// can of worms shut!

		// TODO: 2001:0db8::1428:57ab and 2001:0db8:0:0::1428:57ab are not matched!
		// ip6: thanks to http://blogs.msdn.com/mpoulson/archive/2005/01/10/350037.aspx

		private static readonly List<char> _InvalidChars;

		static Path()
		{
//			_Reserved = new List<string>("CON|PRN|AUX|NUL|COM1|COM2|COM3|COM4|COM5|COM6|COM7|COM8|COM9|LPT1|LPT2|LPT3|LPT4|LPT5|LPT6|LPT7|LPT8|LPT9"
//				.Split('|'));
			_InvalidChars = new List<char>(GetInvalidPathChars());
		}

		///<summary>
		/// Returns whether the path is rooted.
		///</summary>
		///<param name="path">Gets whether the path is rooted or relative.</param>
		///<returns>Whether the path is rooted or not.</returns>
		///<exception cref="ArgumentNullException">If the passed argument is null.</exception>
		public static bool IsRooted(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path == string.Empty) return false;
			return PathInfo.Parse(path).Root != string.Empty;
		}

		/// <summary>
		/// Gets the path root, i.e. e.g. \\?\C:\ if the passed argument is \\?\C:\a\b\c.abc.
		/// </summary>
		/// <param name="path">The path to get the root for.</param>
		/// <returns>The string denoting the root.</returns>
		public static string GetPathRoot(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path == string.Empty) throw new ArgumentException("path was empty.");
			if (ContainsInvalidChars(path)) throw new ArgumentException("path contains invalid characters.");
			return PathInfo.Parse(path).Root;
		}

		private static bool ContainsInvalidChars(string path)
		{
			int c = _InvalidChars.Count;
			int l = path.Length;

			for (int i = 0; i < l; i++)
				for (int j = 0; j < c; j++)
					if (path[i] == _InvalidChars[j])
						return true;
			return false;
		}

		///<summary>
		/// Gets a path without root.
		///</summary>
		///<param name="path"></param>
		///<returns></returns>
		///<exception cref="ArgumentNullException"></exception>
		public static string GetPathWithoutRoot(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path.Length == 0) return string.Empty;
			return path.Substring(GetPathRoot(path).Length);
		}

		///<summary>
		/// Normalize all the directory separation chars.
		/// Also removes empty space in beginning and end of string.
		///</summary>
		///<param name="pathWithAlternatingChars"></param>
		///<returns>The directory string path with all occurrances of the alternating chars
		/// replaced for that specified in <see cref="System.IO.Path.DirectorySeparatorChar"/></returns>
		public static string NormDirSepChars(string pathWithAlternatingChars)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < pathWithAlternatingChars.Length; i++)
				if ((pathWithAlternatingChars[i] == '\\') || (pathWithAlternatingChars[i] == '/'))
					sb.Append(DirectorySeparatorChar);
				else
					sb.Append(pathWithAlternatingChars[i]);
			return sb.ToString().Trim(new[] { ' ' });
		}

		///<summary>
		/// Gets path info (drive and non root path)
		///</summary>
		///<param name="path">The path to get the info from.</param>
		///<returns></returns>
		///<exception cref="ArgumentNullException"></exception>
		public static PathInfo GetPathInfo(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			return PathInfo.Parse(path);
		}

		///<summary>
		/// Gets the full path for a given path.
		///</summary>
		///<param name="path"></param>
		///<returns>The full path string</returns>
		///<exception cref="ArgumentNullException">if path is null</exception>
		public static string GetFullPath(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path.StartsWith("\\\\?\\") || path.StartsWith("\\\\.\\")) return System.IO.Path.GetFullPath(path.Substring(4));
			if (path.StartsWith("\\\\?\\UNC\\")) return System.IO.Path.GetFullPath(path.Substring(8));
			if (path.StartsWith("file:///")) return new Uri(path).LocalPath;
			return System.IO.Path.GetFullPath(path);
		}

		/// <summary>
		/// Removes the last directory/file off the path.
		/// 
		/// For a path "/a/b/c" would return "/a/b"
		/// or for "\\?\C:\folderA\folder\B\C\d.txt" would return "\\?\C:\folderA\folder\B\C"
		/// </summary>
		/// <param name="path">The path string to modify</param>
		/// <returns></returns>
		public static string GetPathWithoutLastBit(string path)
		{
			if (path == null) throw new ArgumentNullException("path");

			var chars = new List<char>(new[] {DirectorySeparatorChar, AltDirectorySeparatorChar});

			bool endsWithSlash = false;
			int secondLast = -1;
			int last = -1;
			char lastType = chars[0];

			for (int i = 0; i < path.Length; i++)
			{
				if (i == path.Length - 1 && chars.Contains(path[i]))
					endsWithSlash = true;

				if (!chars.Contains(path[i])) continue;

				secondLast = last;
				last = i;
				lastType = path[i];
			}

			if (last == -1)
				throw new ArgumentException(string.Format("Could not find a path separator character in the path \"{0}\"", path));

			var res = path.Substring(0, endsWithSlash ? secondLast : last);
			return res == string.Empty ? new string(lastType, 1) : res;
		}

		public static string GetFileName(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path == string.Empty) throw new ArgumentException("path musn't be null", "path");

			if (path.EndsWith("/") || path.EndsWith("\\")) return string.Empty;

			var nonRoot = PathInfo.Parse(path).FolderAndFiles;

			int strIndex;
			
			// resharper is wrong that you can transform this to a ternary operator.
			if ((strIndex = nonRoot.LastIndexOfAny(new[] {
			                                             	DirectorySeparatorChar, AltDirectorySeparatorChar })) != -1)
				return nonRoot.Substring(strIndex+1);
			return nonRoot;
		}

		public static bool HasExtension(string path)
		{
			if (path == null)throw new ArgumentNullException("path");
			if (path == string.Empty) throw new ArgumentException("Path musn't be empty.", "path");
			return GetFileName(path).Length != GetFileNameWithoutExtension(path).Length;
		}

		public static string GetExtension(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path == string.Empty) throw new ArgumentException("Path musn't be empty.", "path");
			var fn = GetFileName(path);
			var lastPeriod = fn.LastIndexOf('.');
			return lastPeriod == -1 ? string.Empty : fn.Substring(lastPeriod + 1);
		}

		public static string GetFileNameWithoutExtension(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			var filename = GetFileName(path);
			var lastPeriod = filename.LastIndexOf('.');
			return lastPeriod == -1 ? filename : filename.Substring(0, lastPeriod);
		}

		public static string GetRandomFileName()
		{
			return System.IO.Path.GetRandomFileName();
		}

		public static char[] GetInvalidPathChars()
		{
			return System.IO.Path.GetInvalidPathChars();
		}

		public static char[] GetInvalidFileNameChars()
		{
			return System.IO.Path.GetInvalidFileNameChars();
		}

		public static char DirectorySeparatorChar
		{
			get { return System.IO.Path.DirectorySeparatorChar; }
		}

		public static char AltDirectorySeparatorChar
		{
			get { return System.IO.Path.AltDirectorySeparatorChar; }
		}

		public static char[] GetDirectorySeparatorChars()
		{
			return new[] { DirectorySeparatorChar, AltDirectorySeparatorChar};
		}
	}
}