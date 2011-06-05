namespace Castle.IO
{
	public interface IFileSystem
	{
		IDirectory GetDirectory(string directoryPath);
		Path GetPath(string path);

		ITemporaryDirectory CreateTempDirectory();
		IDirectory CreateDirectory(string path);
		IFile GetFile(string itemSpec);
		ITemporaryFile CreateTempFile();
		IDirectory GetTempDirectory();
		IDirectory GetCurrentDirectory();
	}
}