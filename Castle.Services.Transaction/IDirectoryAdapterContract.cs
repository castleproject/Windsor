using System;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction
{
	[ContractClassFor(typeof (IDirectoryAdapter))]
	internal abstract class IDirectoryAdapterContract : IDirectoryAdapter
	{
		bool IDirectoryAdapter.Create(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		bool IDirectoryAdapter.Exists(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		void IDirectoryAdapter.Delete(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		bool IDirectoryAdapter.Delete(string path, bool recursively)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		string IDirectoryAdapter.GetFullPath(string relativeDir)
		{
			Contract.Requires(!string.IsNullOrEmpty(relativeDir));
			throw new NotImplementedException();
		}

		string IDirectoryAdapter.MapPath(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalPath));
			Contract.Requires(!string.IsNullOrEmpty(newPath));
			throw new NotImplementedException();
		}
	}
}