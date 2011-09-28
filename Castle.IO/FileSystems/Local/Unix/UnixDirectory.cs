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

using System.IO;
using Mono.Unix;

namespace Castle.IO.FileSystems.Local.Unix
{
	public class UnixDirectory : LocalDirectory
	{
		public UnixDirectory(DirectoryInfo di)
			: base(di)
		{
			GetUnixInfo(di.FullName);
		}

		public UnixDirectory(string directoryPath)
			: base(directoryPath)
		{
			GetUnixInfo(directoryPath);
		}

		public UnixDirectory(Path directoryPath)
			: base(directoryPath.FullPath)
		{
			GetUnixInfo(directoryPath.FullPath);
		}

		public override bool IsHardLink
		{
			get { return UnixDirectoryInfo.IsSymbolicLink; }
		}

		public override IDirectory Target
		{
			get
			{
				if (IsHardLink)
					return new UnixDirectory(((UnixSymbolicLinkInfo) UnixDirectoryInfo).GetContents().FullName);
				return this;
			}
		}

		protected UnixFileSystemInfo UnixDirectoryInfo { get; set; }

		public override IDirectory LinkTo(Path path)
		{
			//UnixDirectoryInfo.CreateSymbolicLink(path);
			return CreateDirectory(path);
		}

		protected override LocalDirectory CreateDirectory(Path path)
		{
			return new UnixDirectory(path);
		}

		protected override LocalDirectory CreateDirectory(DirectoryInfo di)
		{
			return new UnixDirectory(di);
		}

		private void GetUnixInfo(string fullName)
		{
			UnixDirectoryInfo = UnixFileSystemInfo.GetFileSystemEntry(fullName);
		}
	}
}