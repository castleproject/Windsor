using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Castle.Services.Transaction
{
	[ContractClassFor(typeof (IFileAdapter))]
	internal abstract class IFileAdapterContract : IFileAdapter
	{
		public FileStream Create(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		public bool Exists(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		public string ReadAllText(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		public void WriteAllText(string path, string contents)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			Contract.Requires(contents != null, "content't mustn't be null, but it may be empty");
			throw new NotImplementedException();
		}

		public void Delete(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		public FileStream Open(string filePath, FileMode mode)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		public int WriteStream(string toFilePath, Stream fromStream)
		{
			Contract.Requires(!string.IsNullOrEmpty(toFilePath));
			throw new NotImplementedException();
		}

		public string ReadAllText(string path, Encoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		public void Move(string originalFilePath, string newFilePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalFilePath));
			Contract.Requires(!string.IsNullOrEmpty(newFilePath));
			Contract.Requires(Path.GetFileName(originalFilePath) != string.Empty);
			throw new NotImplementedException();
		}

		public IList<string> ReadAllLines(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		public StreamWriter CreateText(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}
	}
}