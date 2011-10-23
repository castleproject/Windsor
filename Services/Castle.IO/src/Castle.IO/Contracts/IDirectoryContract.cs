namespace Castle.IO.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Contract for the IDirectory handle
	/// </summary>
	[ContractClassFor(typeof(IDirectory))]
	internal abstract class IDirectoryContract : IDirectory
	{
		public IDirectory GetDirectory(string directoryName)
		{
			Contract.Requires(directoryName != null);
			Contract.Ensures(Contract.Result<IDirectory>() != null);

			throw new NotImplementedException();
		}

		public IFile GetFile(string fileName)
		{
			Contract.Requires(fileName != null);
			Contract.Ensures(Contract.Result<IFile>() != null);

			throw new NotImplementedException();
		}

		public IEnumerable<IFile> Files()
		{
			Contract.Ensures(Contract.Result<IEnumerable<IFile>>() != null);

			throw new NotImplementedException();
		}

		public IEnumerable<IDirectory> Directories()
		{
			Contract.Ensures(Contract.Result<IEnumerable<IDirectory>>() != null);

			throw new NotImplementedException();
		}

		public IEnumerable<IFile> Files(string filter, SearchScope scope)
		{
			Contract.Requires(filter != null);
			Contract.Ensures(Contract.Result<IEnumerable<IFile>>() != null);

			throw new NotImplementedException();
		}

		public IEnumerable<IDirectory> Directories(string filter, SearchScope scope)
		{
			Contract.Requires(filter != null);
			Contract.Ensures(Contract.Result<IEnumerable<IDirectory>>() != null);

			throw new NotImplementedException();
		}

		public bool IsHardLink
		{
			get { throw new NotImplementedException(); }
		}

		public IDirectory LinkTo(Path path)
		{
			Contract.Requires(path != null);
			Contract.Ensures(Contract.Result<IDirectory>() != null);

			throw new NotImplementedException();
		}

		public IDirectory Target
		{
			get
			{
				Contract.Ensures(Contract.Result<IDirectory>() != null);
				throw new NotImplementedException();
			}
		}

		public IDisposable FileChanges(string filter = "*", bool includeSubdirectories = false, Action<IFile> created = null, Action<IFile> modified = null, Action<IFile> deleted = null, Action<IFile> renamed = null)
		{
			throw new NotImplementedException();
		}

		public abstract IDirectory Create();

		public abstract Path Path { get; }
		public abstract IDirectory Parent { get; }
		public abstract IFileSystem FileSystem { get; }
		public abstract bool Exists { get; }
		public abstract string Name { get; }

		public abstract void Delete();

		public abstract void CopyTo(IFileSystemItem item);

		public abstract void MoveTo(IFileSystemItem item);
	}
}