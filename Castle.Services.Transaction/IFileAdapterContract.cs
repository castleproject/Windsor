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
			Contract.Requires(filePath != null);
			throw new NotImplementedException();
		}

		public bool Exists(string filePath)
		{
			throw new NotImplementedException();
		}

		public string ReadAllText(string path)
		{
			throw new NotImplementedException();
		}

		public void WriteAllText(string path, string contents)
		{
			throw new NotImplementedException();
		}

		public void Delete(string filePath)
		{
			throw new NotImplementedException();
		}

		public FileStream Open(string filePath, FileMode mode)
		{
			throw new NotImplementedException();
		}

		public int WriteStream(string toFilePath, Stream fromStream)
		{
			throw new NotImplementedException();
		}

		public string ReadAllText(string path, Encoding encoding)
		{
			throw new NotImplementedException();
		}

		public void Move(string originalFilePath, string newFilePath)
		{
			throw new NotImplementedException();
		}

		public IList<string> ReadAllLines(string filePath)
		{
			throw new NotImplementedException();
		}

		public StreamWriter CreateText(string filePath)
		{
			throw new NotImplementedException();
		}
	}
}