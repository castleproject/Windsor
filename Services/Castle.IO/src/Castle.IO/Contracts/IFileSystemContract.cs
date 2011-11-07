namespace Castle.IO.Contracts
{
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Contract class for <see cref="IFileSystem"/>.
	/// </summary>
	[ContractClassFor(typeof(IFileSystem))]
	internal abstract class IFileSystemContract : IFileSystem
	{
		public IDirectory GetDirectory(string directoryPath)
		{
			Contract.Requires(directoryPath != null);
			Contract.Ensures(Contract.Result<IDirectory>() != null);

			throw new System.NotImplementedException();
		}

		public IDirectory GetDirectory(Path directoryPath)
		{
			Contract.Requires(directoryPath != null);
			Contract.Ensures(Contract.Result<IDirectory>() != null);
			throw new System.NotImplementedException();
		}

		public Path GetPath(string path)
		{
			Contract.Ensures(Contract.Result<Path>() != null);

			throw new System.NotImplementedException();
		}

		public ITemporaryDirectory CreateTempDirectory()
		{
			Contract.Ensures(Contract.Result<ITemporaryDirectory>() != null);

			throw new System.NotImplementedException();
		}

		public IDirectory CreateDirectory(string path)
		{
			Contract.Requires(path != null);
			Contract.Ensures(Contract.Result<IDirectory>() != null);

			throw new System.NotImplementedException();
		}

		public IDirectory CreateDirectory(Path path)
		{
			Contract.Requires(path != null);
			Contract.Ensures(Contract.Result<IDirectory>() != null);

			throw new System.NotImplementedException();
		}

		public IFile GetFile(string itemSpec)
		{
			Contract.Requires(itemSpec != null);
			Contract.Ensures(Contract.Result<IFile>() != null);

			throw new System.NotImplementedException();
		}

		public ITemporaryFile CreateTempFile()
		{
			Contract.Ensures(Contract.Result<ITemporaryFile>() != null);
			throw new System.NotImplementedException();
		}

		public IDirectory GetTempDirectory()
		{
			Contract.Ensures(Contract.Result<IDirectory>() != null);
			
			throw new System.NotImplementedException();
		}

		public IDirectory GetCurrentDirectory()
		{
			Contract.Ensures(Contract.Result<IDirectory>() != null);
			
			throw new System.NotImplementedException();
		}
	}
}