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

using System.Collections.Generic;
using System.Linq;

namespace Castle.IO
{
	public static class DirectoryExtensions
	{
		private const string SUBFOLDER = "**";

		public static IEnumerable<IDirectory> Directories(this IFileSystem fileSystem, string filter)
		{
			return fileSystem.GetCurrentDirectory().Directories(filter);
		}

		public static IDirectory FindDirectory(this IDirectory directory, string path)
		{
			var child = directory.GetDirectory(path);
			return child.Exists ? child : null;
		}

		public static IFile FindFile(this IDirectory directory, string filename)
		{
			var child = directory.GetFile(filename);
			return child.Exists ? child : null;
		}

		public static IDirectory GetOrCreateDirectory(this IDirectory directory, params string[] childDirectories)
		{
			return childDirectories.Aggregate(directory,
											  (current, childDirectoryName) =>
											  current.GetDirectory(childDirectoryName).MustExist());
		}

		public static IEnumerable<IFile> Files(this IFileSystem fileSystem, string filter)
		{
			return fileSystem.GetCurrentDirectory().Files(filter);
		}

		public static IEnumerable<IFile> Files(this IDirectory directory, string filter)
		{
			var pathSegments = GetFilterPaths(filter).ToList();

			if (pathSegments.Count() == 1)
				return directory.Files(filter, SearchScope.CurrentOnly);

			return GetFileSpecCore(directory, pathSegments, 0).ToList();
		}

		private static IEnumerable<IFile> GetFileSpecCore(IDirectory directory, IList<string> segments, int position)
		{
			var segment = segments[position];
			if (position == segments.Count - 1)
			{
				return directory.Files(segment);
			}
			if (segment == SUBFOLDER)
			{
				var isNextToLastSegment = position + 1 == segments.Count - 1;

				return isNextToLastSegment
				       	? directory.Files(segments[position + 1], SearchScope.SubFolders)
				       	: directory.Directories(segments[position + 1], SearchScope.SubFolders)
				       	  	.SelectMany(x => GetFileSpecCore(x, segments, position + 2));
			}
			return directory.Directories(segment, SearchScope.CurrentOnly)
				.SelectMany(x => GetFileSpecCore(x, segments, position + 1));
		}

		private static IEnumerable<IDirectory> GetDirectorySpecCore(IDirectory directory, IList<string> segments, int position)
		{
			var segment = segments[position];
			if (position == segments.Count - 1)
				return directory.Directories(segment);

			if (segment == SUBFOLDER)
			{
				var isNextToLastSegment = position + 1 == segments.Count - 1;
				return isNextToLastSegment
				       	? directory.Directories(segments[position + 1], SearchScope.SubFolders)
				       	: directory.Directories(segments[position + 1], SearchScope.SubFolders)
				       	  	.SelectMany(x => GetDirectorySpecCore(x, segments, position + 2));
			}
			return directory.Directories(segment, SearchScope.CurrentOnly)
				.SelectMany(x => GetDirectorySpecCore(x, segments, position + 1));
		}

		private static IEnumerable<string> GetFilterPaths(string filter)
		{
			var lastWasSubFolder = false;
			
			var path = new Path(filter);

			foreach (var segment in path.Segments)
			{
				if (segment == SUBFOLDER)
					if (!lastWasSubFolder)
						lastWasSubFolder = true;
					else
						continue;
				else
					lastWasSubFolder = false;

				yield return segment;
			}
		}

		public static IEnumerable<IDirectory> Directories(this IDirectory directory, string filter)
		{
			var pathSegments = GetFilterPaths(filter).ToList();

			if (pathSegments.Count == 1)
				return directory.Directories(filter, SearchScope.CurrentOnly);
			
			return GetDirectorySpecCore(directory, pathSegments, 0);
		}

		public static IEnumerable<IDirectory> AncestorsAndSelf(this IDirectory directory)
		{
			do
			{
				yield return directory;
				directory = directory.Parent;
			} while (directory != null);
		}
	}
}