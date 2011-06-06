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
using Castle.IO.FileSystems.Local.Unix;
using Castle.IO.FileSystems.Local.Win32;

namespace Castle.IO.FileSystems.Local
{
	public abstract class LocalFileSystem : AbstractFileSystem
	{
		private static LocalFileSystem _instance;
		private static readonly object _syncRoot = new object();

		public static LocalFileSystem Instance
		{
			get
			{
				if (_instance == null)
					lock (_syncRoot)
						if (_instance == null)
							_instance = CreatePlatformSpecificInstance();
				return _instance;
			}
		}

		private static LocalFileSystem CreatePlatformSpecificInstance()
		{
			var platformId = (int) Environment.OSVersion.Platform;
			
			if (platformId == (int) PlatformID.Win32NT)
				return CreateWin32FileSystem();
			
			if (platformId == 4 || platformId == 128 || platformId == (int) PlatformID.MacOSX)
				return UnixFileSystem();

			throw new NotSupportedException("Platform not supported");
		}

		private static LocalFileSystem CreateWin32FileSystem()
		{
			return new Win32FileSystem();
		}

		private static LocalFileSystem UnixFileSystem()
		{
			return new UnixFileSystem();
		}

		public override IDirectory CreateDirectory(string path)
		{
			return GetDirectory(path).Create();
		}

		public override IDirectory CreateDirectory(Path path)
		{
			return GetDirectory(path).Create();
		}

		public override ITemporaryDirectory CreateTempDirectory()
		{
			var tempPath = Path.GetTempPath();
			var dirName = Path.GetRandomFileName();

			return new TemporaryDirectory(CreateDirectory(tempPath.Combine(dirName)));
		}

		public override ITemporaryFile CreateTempFile()
		{
			return new TemporaryLocalFile(Path.GetTempFileName(), di => CreateDirectory(di.FullName));
		}

		public override IFile GetFile(string filePath)
		{
			return new LocalFile(Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, filePath)),
			                     di => CreateDirectory(di.FullName));
		}

		public override Path GetPath(string path)
		{
			return new Path(path);
		}

		public override IDirectory GetCurrentDirectory()
		{
			return GetDirectory(Environment.CurrentDirectory);
		}
	}
}