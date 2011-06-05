namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	public enum CopyProgressCallbackReason : uint
	{
		CALLBACK_CHUNK_FINISHED = 0x00000000,
		CALLBACK_STREAM_SWITCH = 0x00000001
	}
}