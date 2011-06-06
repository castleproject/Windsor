using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			return (NativeFileShare)(uint)fileShare;
		}

		public static NativeFileMode ToNative(this FileMode fileMode)
		{
			if (fileMode != FileMode.Append)
				return (NativeFileMode)(uint)fileMode;
			return (NativeFileMode)(uint)FileMode.OpenOrCreate;
		}

		public static NativeFileOptions ToNative(this FileOptions fileOptions)
		{
			return (NativeFileOptions)(uint)fileOptions;
		}
	}
}
