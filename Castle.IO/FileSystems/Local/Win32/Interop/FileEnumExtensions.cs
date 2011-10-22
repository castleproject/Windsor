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

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	public static class FileEnumExtensions
	{
		public static NativeFileAccess ToNative(this FileAccess fileAccess)
		{
			switch (fileAccess)
			{
				case FileAccess.Read:
					return NativeFileAccess.GenericRead;
				case FileAccess.Write:
					return NativeFileAccess.GenericWrite;
				case FileAccess.ReadWrite:
					return NativeFileAccess.GenericRead | NativeFileAccess.GenericWrite;
				default:
					throw new ArgumentOutOfRangeException("fileAccess");
			}
		}

		public static NativeFileShare ToNative(this FileShare fileShare)
		{
			return (NativeFileShare) (uint) fileShare;
		}

		public static NativeFileMode ToNative(this FileMode fileMode)
		{
			if (fileMode != FileMode.Append)
				return (NativeFileMode) (uint) fileMode;
			return (NativeFileMode) (uint) FileMode.OpenOrCreate;
		}

		public static NativeFileOptions ToNative(this FileOptions fileOptions)
		{
			return (NativeFileOptions) (uint) fileOptions;
		}
	}
}