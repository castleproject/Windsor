using System;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	[Serializable]
	public enum FINDEX_SEARCH_OPS
	{
		FindExSearchNameMatch = 0,
		FindExSearchLimitToDirectories = 1,
		FindExSearchLimitToDevices = 2,
		FindExSearchMaxSearchOp = 3
	}
}