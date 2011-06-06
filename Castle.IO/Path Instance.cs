#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.IO.Internal;

namespace Castle.IO
{
	// wow, I am actually using the partial keyword.
	// the reason for this is that I think the static
	// methods corresponding to System.IO.Path should
	// be in their own class declaration, yet
	// I'm reluctant to rename this class or 
	// to rename the static method carrying class
	// for that matter!

	/// <summary>
	/// Immutable value object for dealing with paths. A path as an object,
	/// is the idea.
	/// </summary>
	public partial class Path : IEquatable<Path>
	{
		private readonly string _OriginalPath;
		private readonly PathInfo _PathInfo;
		private readonly IList<string> _Segments;

		public Path(string path)
		{
			Contract.Requires(path != null);
			Contract.Requires(!string.IsNullOrEmpty(path));

			_OriginalPath = path;
			_PathInfo = PathInfo.Parse(path);
			_Segments = GenerateSegments(path);
		}

		/// <summary>
		/// Gets the drive + directory path. If the given path ends with a slash,
		/// the last bit is also included in this property, otherwise, not.
		/// </summary>
		public string DriveAndDirectory
		{
			get
			{
				var dir = IsDirectoryPath 
					? _PathInfo.FolderAndFiles 
					: (_Segments.Count == 1 ? _Segments[0] : _Segments.Skip(1).Aggregate(_Segments[0], System.IO.Path.Combine));
				return _PathInfo.Drive.Combine(dir);
			}
		}

		public bool IsRooted { get { return _PathInfo.IsRooted; } }

		private static IList<string> GenerateSegments(string path)
		{
			return path
				.Split(new[] { DirectorySeparatorChar, AltDirectorySeparatorChar}, 
					   StringSplitOptions.RemoveEmptyEntries)
				.Except(new[]{"?"})
				.ToList();
		}

		public string FullPath { get { return _OriginalPath; } }

		public IEnumerable<string> Segments { get { return _Segments; } }

		/// <summary>
		/// Gets whether it's garantueed that the path is a directory (it is of its
		/// last character is a directory separator character).
		/// </summary>
		public bool IsDirectoryPath
		{
			get
			{
				return FullPath.EndsWith(DirectorySeparatorChar + "") ||
				       FullPath.EndsWith(AltDirectorySeparatorChar + "");
			}
		}

		public Path Combine(params string[] paths)
		{
			var combinedPath = paths.Aggregate(FullPath, System.IO.Path.Combine);
			return new Path(combinedPath);
		}

		public Path Combine(params Path[] paths)
		{
			return Combine(paths.Select(p => p.FullPath).ToArray());
		}

		#region Equality operators

		public bool Equals(Path other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other._PathInfo.Equals(_PathInfo);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is Path)) return false;
			return Equals((Path) obj);
		}

		public override int GetHashCode()
		{
			return _PathInfo.GetHashCode();
		}

		public override string ToString()
		{
			return _OriginalPath;
		}

		public static bool operator ==(Path left, Path right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Path left, Path right)
		{
			return !Equals(left, right);
		}

		//public static implicit operator string(Path path)
		//{
		//    return path.ToString();
		//}

		#endregion

		/// <summary>
		/// Yields a new path instance from the current data object
		/// and the object passed as the parameter 'path'.
		/// </summary>
		/// <param name="toBasePath">The path to make the invokee relative to.</param>
		/// <returns>A new path that is relative to the passed path.</returns>
		public Path MakeRelative(Path toBasePath)
		{
			if (!IsRooted)
				return this;

			var leftOverSegments = new List<string>();
			var relativeSegmentCount = 0;

			var thisEnum = Segments.GetEnumerator();
			var rootEnum = toBasePath.Segments.GetEnumerator();

			bool thisHasValue;
			bool rootHasValue;
			do
			{
				thisHasValue = thisEnum.MoveNext();
				rootHasValue = rootEnum.MoveNext();

				if (thisHasValue && rootHasValue)
				{
					if (thisEnum.Current.Equals(rootEnum.Current, StringComparison.OrdinalIgnoreCase))
						continue;
				}
				if (thisHasValue)
				{
					leftOverSegments.Add(thisEnum.Current);
				}
				if (rootHasValue)
					relativeSegmentCount++;
			} while (thisHasValue || rootHasValue);

			var relativeSegment = Enumerable.Repeat("..", relativeSegmentCount).Aggregate("", System.IO.Path.Combine);
			var finalSegment = System.IO.Path.Combine(relativeSegment, leftOverSegments.Aggregate("", System.IO.Path.Combine));
			return new Path(finalSegment);
		}
	}
}