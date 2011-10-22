namespace Castle.IO.Contracts
{
	using System.Diagnostics.Contracts;

	[ContractClassFor(typeof(IFileSystemItem))]
	public class IFileSystemItemContract : IFileSystemItem
	{
		public Path Path
		{
			get
			{
				Contract.Ensures(Contract.Result<Path>() != null);
				throw new System.NotImplementedException();
			}
		}

		public IDirectory Parent
		{
			get
			{
				Contract.Ensures(
					Contract.Result<IDirectory>() == null 
					|| Contract.Result<IDirectory>() != null);

				throw new System.NotImplementedException();
			}
		}

		public IFileSystem FileSystem
		{
			get
			{
				Contract.Ensures(Contract.Result<IFileSystemItem>() != null);
				throw new System.NotImplementedException();
			}
		}

		public bool Exists
		{
			get { throw new System.NotImplementedException(); }
		}

		public string Name
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				throw new System.NotImplementedException();
			}
		}

		public void Delete()
		{
			throw new System.NotImplementedException();
		}

		public void CopyTo(IFileSystemItem item)
		{
			Contract.Requires(item != null);
			throw new System.NotImplementedException();
		}

		public void MoveTo(IFileSystemItem item)
		{
			Contract.Requires(item != null);
			throw new System.NotImplementedException();
		}
	}
}