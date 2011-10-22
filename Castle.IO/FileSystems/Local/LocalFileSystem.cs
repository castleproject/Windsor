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
using Castle.IO.FileSystems.Local.Unix;
using Castle.IO.FileSystems.Local.Win32;

namespace Castle.IO.FileSystems.Local
{
	using System.Diagnostics.Contracts;

	public abstract class LocalFileSystem : AbstractFileSystem
	{
		private static volatile LocalFileSystem instance;
		private static readonly object syncRoot = new object();

		/// <summary>
		/// Gets an instance of the local file system. This property
		/// is a singleton.
		/// </summary>
		/// <exception cref="PlatformNotSupportedException">
		/// If your platform isn't supported by this API.
		/// Currently Windows, OSX and Unix platforms are supported.
		/// </exception>
		public static LocalFileSystem Instance
		{
			get
			{
				Contract.Ensures(Contract.Result<LocalFileSystem>() != null);

				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = CreatePlatformSpecificInstance();

				return instance;
			}
		}

		private static LocalFileSystem CreatePlatformSpecificInstance()
		{
			Contract.Ensures(Contract.Result<LocalFileSystem>() != null);

			var platformId = (int) Environment.OSVersion.Platform;

			if (platformId == (int) PlatformID.Win32NT)
				return CreateWin32FileSystem();

			if (platformId == 4 || platformId == 128 || platformId == (int) PlatformID.MacOSX)
				return UnixFileSystem();

			throw new NotSupportedException("Platform not supported");
		}

		private static LocalFileSystem CreateWin32FileSystem()
		{
			Contract.Ensures(Contract.Result<LocalFileSystem>() != null);
			
			return new Win32FileSystem();
		}

		private static LocalFileSystem UnixFileSystem()
		{
			Contract.Ensures(Contract.Result<LocalFileSystem>() != null);

			return new UnixFileSystem();
		}

		public override IDirectory CreateDirectory(string path)
		{
			return GetDirectory(path).Create();
		}

		public override IDirectory CreateDirectory(Path path)
		{
			Contract.Ensures(Contract.Result<IDirectory>() != null);

			return GetDirectory(path).Create();
		}

		public override ITemporaryDirectory CreateTempDirectory()
		{
			Contract.Ensures(Contract.Result<ITemporaryDirectory>() != null);

			var tempPath = Path.GetTempPath();
			var dirName = Path.GetRandomFileName();

			return new TemporaryDirectory(CreateDirectory(tempPath.Combine(dirName)));
		}

		public override ITemporaryFile CreateTempFile()
		{
			Contract.Ensures(Contract.Result<ITemporaryFile>() != null);

			return new TemporaryLocalFile(Path.GetTempFileName(), CreateDirectory);
		}

		public override IFile GetFile(string filePath)
		{
			Contract.Requires(filePath != null);
			Contract.Ensures(Contract.Result<IFile>() != null);

			return new LocalFile(Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, filePath)), CreateDirectory);
		}

		public override Path GetPath(string path)
		{
			Contract.Requires(path != null);

			return new Path(path);
		}

		public override IDirectory GetCurrentDirectory()
		{
			return GetDirectory(Environment.CurrentDirectory);
		}
	}
}