using System.Collections.Generic;
using System.Linq;

namespace Castle.IO
{
	public static class DirectoryExtensions
	{
		const string SUBFOLDER = "**";
		public static IEnumerable<IDirectory> Directories(this IFileSystem fileSystem, string filter)
		{
			return fileSystem.GetCurrentDirectory().Directories(filter);
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
		static IEnumerable<IFile> GetFileSpecCore(IDirectory directory, IList<string> segments, int position)
		{
			var segment = segments[position];
			if (position == segments.Count-1)
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
		static IEnumerable<IDirectory> GetDirectorySpecCore(IDirectory directory, IList<string> segments, int position)
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
		static IEnumerable<string> GetFilterPaths(string filter)
		{
			bool lastWasSubFolder = false;
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
	}
}