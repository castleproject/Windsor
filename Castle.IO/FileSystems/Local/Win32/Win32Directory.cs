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
using Castle.IO.FileSystems.Local.Win32.Interop;

namespace Castle.IO.FileSystems.Local.Win32
{
	public class Win32Directory : LocalDirectory
	{
		public Win32Directory(DirectoryInfo directory) : base(directory)
		{
		}

		public Win32Directory(string directoryPath) : base(directoryPath)
		{
		}

		public override void Delete()
		{
			if (IsHardLink)
				JunctionPoint.Delete(Path.FullPath);
			else
				base.Delete();
		}

		public override bool IsHardLink
		{
			get { return JunctionPoint.Exists(Path.FullPath); }
		}

		public override IDirectory LinkTo(Path path)
		{
			//path = NormalizeDirectoryPath(path);
			JunctionPoint.Create(GetDirectory(path.FullPath), Path.FullPath, true);
			return new Win32Directory(path.FullPath);
		}

		protected override LocalDirectory CreateDirectory(DirectoryInfo di)
		{
			return new Win32Directory(di);
		}

		protected override LocalDirectory CreateDirectory(Path path)
		{
			return new Win32Directory(path.FullPath);
		}

		public override IDirectory Target
		{
			get { return IsHardLink ? new Win32Directory(JunctionPoint.GetTarget(Path.FullPath)) : this; }
		}
	}
}