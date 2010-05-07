using System;
using Castle.Services.Transaction.IO;

namespace Castle.Services.Transaction
{
	///<summary>
	/// Interface for a transaction acting on a file.
	///</summary>
	public interface IFileTransaction : IFileAdapter, IDirectoryAdapter, ITransaction, IDisposable
	{
	}
}