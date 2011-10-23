namespace Castle.IO.Contracts
{
	using System.Diagnostics.Contracts;

	[ContractClassFor(typeof(IFileSystemItem<>))]
	internal abstract class IFileSystemItemTContract<T> : IFileSystemItem<T>
		where T : IFileSystemItem
	{
		public T Create()
		{
			Contract.Ensures(!object.ReferenceEquals(Contract.Result<T>(), null));
			throw new System.NotImplementedException();
		}

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