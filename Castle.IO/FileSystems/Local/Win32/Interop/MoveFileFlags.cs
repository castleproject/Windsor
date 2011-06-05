using System;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	/// <summary>
	/// 	This enumeration states options for moving a file.
	/// 	http://msdn.microsoft.com/en-us/library/aa365241%28VS.85%29.aspx
	/// </summary>
	[Flags, Serializable]
	public enum MoveFileFlags : uint
	{
		/// <summary>
		/// 	If the file is to be moved to a different volume, the function simulates the move by using the CopyFile  and DeleteFile  functions.
		/// 	This value cannot be used with MOVEFILE_DELAY_UNTIL_REBOOT.
		/// </summary>
		CopyAllowed = 0x2,

		/// <summary>
		/// 	Reserved for future use.
		/// </summary>
		CreateHardlink = 0x10,

		/// <summary>
		/// 	The system does not move the file until the operating system is restarted. The system moves the file immediately after AUTOCHK is executed, but before creating any paging files. Consequently, this parameter enables the function to delete paging files from previous startups.
		/// 	This value can only be used if the process is in the context of a user who belongs to the administrators group or the LocalSystem account.
		/// 	This value cannot be used with MOVEFILE_COPY_ALLOWED.
		/// 	The write operation to the registry value as detailed in the Remarks section is what is transacted. The file move is finished when the computer restarts, after the transaction is complete.
		/// </summary>
		DelayUntilReboot = 0x4,

		/// <summary>
		/// 	If a file named lpNewFileName exists, the function replaces its contents with the contents of the lpExistingFileName file.
		/// 	This value cannot be used if lpNewFileName or lpExistingFileName names a directory.
		/// </summary>
		ReplaceExisting = 0x1,

		/// <summary>
		/// 	A call to MoveFileTransacted means that the move file operation is complete when the commit operation is completed. This flag is unnecessary; there are no negative affects if this flag is specified, other than an operation slowdown. The function does not return until the file has actually been moved on the disk.
		/// 	Setting this value guarantees that a move performed as a copy and delete operation is flushed to disk before the function returns. The flush occurs at the end of the copy operation.
		/// 	This value has no effect if MOVEFILE_DELAY_UNTIL_REBOOT is set.
		/// </summary>
		WriteThrough = 0x8
	}
}