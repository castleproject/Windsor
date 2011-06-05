using System;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	[Serializable]
	public enum NativeFileMode : uint
	{
		CREATE_NEW = 1,
		CREATE_ALWAYS = 2,
		OPEN_EXISTING = 3,
		OPEN_ALWAYS = 4,
		TRUNCATE_EXISTING = 5
	}
}