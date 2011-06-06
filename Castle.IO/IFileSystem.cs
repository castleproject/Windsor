namespace Castle.IO
{
	public interface IFileSystem
	{
		IDirectory GetDirectory(string directoryPath);

		IDirectory GetDirectory(Path directoryPath);
		
		Path GetPath(string path);

		ITemporaryDirectory CreateTempDirectory();
		
		IDirectory CreateDirectory(string path);
		
		IDirectory CreateDirectory(Path path);
		
		IFile GetFile(string itemSpec);
		
		ITemporaryFile CreateTempFile();
		
		IDirectory GetTempDirectory();

		IDirectory GetCurrentDirectory();
	}
}