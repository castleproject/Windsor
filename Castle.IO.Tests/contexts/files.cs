using System.IO;

namespace Castle.IO.Tests
{
	public abstract class files<T> : file_system_ctxt<T> where T : IFileSystem, new()
	{
		protected IFile write_to_file(byte[] value = null, FileMode mode = FileMode.Create, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
		{
			value = value ?? new[] { (byte)0 };

			var TempFile = TempDir.GetFile(Path.GetRandomFileName());

			TempFile.Exists.ShouldBeFalse();

			using (var stream = TempFile.Open(mode, access, share))
				stream.Write(value);

			return TempFile;
		}
	}
}
