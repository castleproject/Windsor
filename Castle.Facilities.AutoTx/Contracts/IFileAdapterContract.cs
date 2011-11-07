namespace Castle.Facilities.Transactions.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using Path = IO.Path;

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

		string IFileAdapter.ReadAllText(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		void IFileAdapter.WriteAllText(string path, string contents)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			Contract.Requires(contents != null, "content't mustn't be null, but it may be empty");
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

		int IFileAdapter.WriteStream(string toFilePath, Stream fromStream)
		{
			Contract.Requires(!string.IsNullOrEmpty(toFilePath));
			throw new NotImplementedException();
		}

		string IFileAdapter.ReadAllText(string path, Encoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		void IFileAdapter.Move(string originalFilePath, string newFilePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalFilePath));
			Contract.Requires(!string.IsNullOrEmpty(newFilePath));
			Contract.Requires(Path.GetFileName(originalFilePath).Length > 0);
			throw new NotImplementedException();
		}

		IList<string> IFileAdapter.ReadAllLines(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		StreamWriter IFileAdapter.CreateText(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		IEnumerable<string> IFileAdapter.ReadAllLinesEnumerable(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}
	}
}