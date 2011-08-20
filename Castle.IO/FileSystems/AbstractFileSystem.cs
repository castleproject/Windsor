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

namespace Castle.IO.FileSystems
{
	public abstract class AbstractFileSystem : IFileSystem
	{
		public abstract ITemporaryDirectory CreateTempDirectory();
		public abstract IDirectory CreateDirectory(string path);
		public abstract IDirectory CreateDirectory(Path path);
		public abstract ITemporaryFile CreateTempFile();
		public abstract IDirectory GetDirectory(string directoryPath);
		public abstract IDirectory GetDirectory(Path directoryPath);
		public abstract IFile GetFile(string itemSpec);
		public abstract Path GetPath(string path);
		public abstract IDirectory GetTempDirectory();
		public abstract IDirectory GetCurrentDirectory();
	}
}