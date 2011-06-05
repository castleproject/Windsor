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
		private readonly string _NormalizedPath;

		public Path(string fullPath)
		{
			Contract.Requires(fullPath != null);

			FullPath = fullPath;

			IsRooted = IsPathRooted(fullPath);

			Segments = GenerateSegments(fullPath);
			_NormalizedPath = NormalizePath(fullPath);
		}

		public string DirectoryName
		{
			get { return IsDirectoryPath ? _NormalizedPath : GetDirectoryName(FullPath); }
		}

		public bool IsRooted { get; private set; }

		private static IEnumerable<string> GenerateSegments(string path)
		{
			return path.Split(new[] { DirectorySeparatorChar, AltDirectorySeparatorChar},
				           StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();
		}

		public string FullPath { get; private set; }

		public IEnumerable<string> Segments { get; private set; }

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

		#region Equality operators

		public bool Equals(Path other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return NormalizePath(other.FullPath).Equals(_NormalizedPath, StringComparison.OrdinalIgnoreCase);
		}

		private static string NormalizePath(string fullPath)
		{
			return string.Join("" + DirectorySeparatorChar, GenerateSegments(fullPath).ToArray());
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
			return (_NormalizedPath != null ? _NormalizedPath.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return _NormalizedPath;
		}

		public static bool operator ==(Path left, Path right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Path left, Path right)
		{
			return !Equals(left, right);
		}

		public static implicit operator string(Path path)
		{
			return path.ToString();
		}

		#endregion

		public Path MakeRelative(Path path)
		{
			if (!IsRooted)
				return this;

			var leftOverSegments = new List<string>();
			var relativeSegmentCount = 0;

			var thisEnum = Segments.GetEnumerator();
			var rootEnum = path.Segments.GetEnumerator();

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