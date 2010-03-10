using System;

namespace Castle.Services.Transaction
{
	///<summary>
	/// A resource adapter for a file transaction.
	///</summary>
	public class FileResourceAdapter : IResource, IDisposable
	{
		private readonly IFileTransaction _Transaction;

		///<summary>
		/// c'tor
		///</summary>
		///<param name="transaction"></param>
		public FileResourceAdapter(IFileTransaction transaction)
		{
			_Transaction = transaction;
		}

		/// <summary>
		/// Gets the transaction this resouce adapter is an
		/// adapter for.
		/// </summary>
		public IFileTransaction Transaction
		{
			get { return _Transaction; }
		}

		/// <summary>
		/// Implementors should start the
		///             transaction on the underlying resource
		/// </summary>
		public void Start()
		{
			_Transaction.Begin();
		}

		/// <summary>
		/// Implementors should commit the
		///             transaction on the underlying resource
		/// </summary>
		public void Commit()
		{
			_Transaction.Commit();
		}

		/// <summary>
		/// Implementors should rollback the
		///             transaction on the underlying resource
		/// </summary>
		public void Rollback()
		{
			_Transaction.Rollback();
		}

		public void Dispose()
		{
			_Transaction.Dispose();
		}
	}
}