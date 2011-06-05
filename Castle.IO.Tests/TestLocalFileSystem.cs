using Castle.IO;
using Castle.IO.FileSystems.Local;

namespace OpenWrap.Tests.IO
{
	public class TestLocalFileSystem : IFileSystem
	{
		public IDirectory GetDirectory(string directoryPath)
		{
			return _local.GetDirectory(directoryPath);
		}

		public Path GetPath(string path)
		{
			return _local.GetPath(path);
		}

		public ITemporaryDirectory CreateTempDirectory()
		{
			return _local.CreateTempDirectory();
		}

		public IDirectory CreateDirectory(string path)
		{
			return _local.CreateDirectory(path);
		}

		public IFile GetFile(string itemSpec)
		{
			return _local.GetFile(itemSpec);
		}

		public ITemporaryFile CreateTempFile()
		{
			return _local.CreateTempFile();
		}

		public IDirectory GetTempDirectory()
		{
			return _local.GetTempDirectory();
		}

		public IDirectory GetCurrentDirectory()
		{
			return _local.GetCurrentDirectory();
		}

		IFileSystem _local = LocalFileSystem.Instance;
	}
}