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
using System.IO;

namespace Castle.IO.FileSystems
{
	internal static class Validate
	{
		internal static void FileAccess(FileMode fileMode, FileAccess fileAccess)
		{
			// exception if:

			// !write && append
			// !write && create
			// !write && createNew
			// !write && truncate

			var noWrite = (fileAccess & System.IO.FileAccess.Write) == 0;
			if (noWrite && fileMode == FileMode.CreateNew)
				throw new ArgumentException(string.Format(
					"Can only open files in {0} mode when requesting FileAccess.Write access.", fileMode));

			if (noWrite && fileMode == FileMode.Truncate)
				throw new IOException("Cannot truncate a file if file mode doesn't include WRITE.");

			// or if:
			// readwrite && append
			// read && append

			if (fileAccess == System.IO.FileAccess.Read && fileMode == FileMode.Append)
				throw new ArgumentException("Cannot open file in read-mode when having FileMode.Append");

			//if (
			//    ((fileMode == FileMode.Append) && fileAccess != FileAccess.Write) ||
			//    ((fileMode == FileMode.CreateNew || fileMode == FileMode.Create || fileMode == FileMode.Truncate)
			//     && (fileAccess != FileAccess.Write && fileAccess != FileAccess.ReadWrite)) ||
			//    false //((Exists && fileMode == FileMode.OpenOrCreate && fileAccess == FileAccess.Write))
			//    )
		}
	}
}