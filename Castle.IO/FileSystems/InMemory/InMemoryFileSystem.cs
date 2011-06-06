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
using System.Linq;
using System.Threading;

namespace Castle.IO.FileSystems.InMemory
{
	public class InMemoryFileSystem : IFileSystem
	{
		private readonly object _syncRoot = new object();
		public Dictionary<string, InMemoryDirectory> Directories { get; private set; }

		public InMemoryFileSystem()
		{
			Directories = new Dictionary<string, InMemoryDirectory>(StringComparer.OrdinalIgnoreCase);
			CurrentDirectory = new Path(@"c:\");
			Notifier = new InMemoryFileSystemNotifier();
		}

		public InMemoryFileSystemNotifier Notifier { get; set; }

		private InMemoryDirectory GetRoot(string path)
		{
			InMemoryDirectory directory;
			
			lock (Directories)
			{
				if (!Directories.TryGetValue(path, out directory))
				{
					Directories.Add(path, directory = new InMemoryDirectory(this, path));
				}
			}

			return directory;
		}

		public IDirectory GetDirectory(string directoryPath)
		{
			return GetDirectory(new Path(directoryPath));
		}

		public IDirectory GetDirectory(Path directoryPath)
		{
			directoryPath = EnsureTerminatedByDirectorySeparator(directoryPath);

			var resolvedDirectoryPath = CurrentDirectory.Combine(directoryPath);

			var segments = resolvedDirectoryPath.Segments;

			return segments
				.Skip(1)
				.Aggregate((IDirectory)GetRoot(segments.First()),
						   (current, segment) => current.GetDirectory(segment));
		}

		private static Path EnsureTerminatedByDirectorySeparator(Path possibleDirectoryPath)
		{
			return possibleDirectoryPath.IsDirectoryPath
			       	? possibleDirectoryPath
			       	: new Path(possibleDirectoryPath.FullPath + Path.DirectorySeparatorChar);
		}

		public IFile GetFile(string filePath)
		{
			var resolvedFilePath = CurrentDirectory.Combine(filePath);
			var pathSegments = resolvedFilePath.Segments;

			var ownerFolder = pathSegments
				.Skip(1).Take(pathSegments.Count() - 2)
				.Aggregate((IDirectory) GetRoot(pathSegments.First()),
				           (current, segment) => current.GetDirectory(segment));

			return ownerFolder.GetFile(pathSegments.Last());
		}

		public Path GetPath(string path)
		{
			return new Path(path);
		}

		public ITemporaryDirectory CreateTempDirectory()
		{
			var sysTemp = (InMemoryDirectory) GetTempDirectory();

			var tempDirectory = new InMemoryTemporaryDirectory(this,
			                                                   sysTemp.Path.Combine(Path.GetRandomFileName()).FullPath)
			                    	{
			                    		Exists = true,
			                    		Parent = sysTemp
			                    	};
			lock (sysTemp.ChildDirectories)
			{
				sysTemp.ChildDirectories.Add(tempDirectory);
			}
			return tempDirectory;
		}

		public IDirectory CreateDirectory(string path)
		{
			return GetDirectory(path).MustExist();
		}

		public IDirectory CreateDirectory(Path path)
		{
			return GetDirectory(path).MustExist();
		}

		public ITemporaryFile CreateTempFile()
		{
			var tempDirectory = (InMemoryDirectory) GetTempDirectory();
			var tempFile = new InMemoryTemporaryFile(tempDirectory.Path.Combine(Path.GetRandomFileName()).ToString())
			               	{
			               		Exists = true,
			               		FileSystem = this,
			               		Parent = tempDirectory
			               	};
			tempDirectory.Create();
			tempDirectory.ChildFiles.Add(tempFile);

			return tempFile;
		}

		private IDirectory _systemTempDirectory;

		public IDirectory GetTempDirectory()
		{
			if (_systemTempDirectory == null)
			{
				lock (_syncRoot)
				{
					Thread.MemoryBarrier();
					if (_systemTempDirectory == null)
						_systemTempDirectory = GetDirectory(Path.GetTempPath());
				}
			}
			return _systemTempDirectory;
		}

		public IDirectory GetCurrentDirectory()
		{
			return GetDirectory(CurrentDirectory);
		}

		public Path CurrentDirectory { get; set; }
	}
}