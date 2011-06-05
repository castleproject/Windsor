using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Castle.IO.Contracts
{
	[ContractClassFor(typeof (IFileAdapter))]
	internal abstract class IFileAdapterContract : IFileAdapter
	{
		FileStream IFileAdapter.Create(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		bool IFileAdapter.Exists(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		void IFileAdapter.Delete(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		FileStream IFileAdapter.Open(string filePath, FileMode mode)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		FileStream IFileAdapter.OpenWrite(string path)
		{
			throw new NotImplementedException();
		}

		void IFileAdapter.Move(string originalFilePath, string newFilePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalFilePath));
			Contract.Requires(!string.IsNullOrEmpty(newFilePath));
			Contract.Requires(Internal.Path.GetFileName(originalFilePath).Length > 0);
			throw new NotImplementedException();
		}
		StreamWriter IFileAdapter.CreateText(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}
	}
}