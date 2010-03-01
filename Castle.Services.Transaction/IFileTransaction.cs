using System;
using Castle.Core.IO;

namespace Castle.Services.Transaction.IO
{
	///<summary>
	/// Interface for a transaction acting on a file.
	///</summary>
	public interface IFileTransaction : IFileAdapter, IDirectoryAdapter, ITransaction, IDisposable
	{
	}
}