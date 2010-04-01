using System;

namespace Castle.Services.Transaction.IO
{
	///<summary>
	/// Adapter base class for the file and directory adapters.
	///</summary>
	public abstract class TxAdapterBase
	{
		private readonly bool _AllowOutsideSpecifiedFolder;
		private readonly string _SpecifiedFolder;
		private ITransactionManager _TxManager;
		private bool _UseTransactions;
		private bool _OnlyJoinExisting;

		protected TxAdapterBase(bool constrainToSpecifiedDir,
		                        string specifiedDir)
		{
			if (constrainToSpecifiedDir && specifiedDir == null) throw new ArgumentNullException("specifiedDir");
			if (constrainToSpecifiedDir && specifiedDir == string.Empty)
				throw new ArgumentException("The specifified directory was empty.");

			_AllowOutsideSpecifiedFolder = !constrainToSpecifiedDir;
			_SpecifiedFolder = specifiedDir;
		}

		/// <summary>
		/// Gets the transaction manager, if there is one, or sets it.
		/// </summary>
		public ITransactionManager TxManager
		{
			get { return _TxManager; }
			set { _TxManager = value; }
		}

		///<summary>
		/// Gets/sets whether to use transactions.
		///</summary>
		public bool UseTransactions
		{
			get { return _UseTransactions; }
			set { _UseTransactions = value; }
		}

		public bool OnlyJoinExisting
		{
			get { return _OnlyJoinExisting; }
			set { _OnlyJoinExisting = value; }
		}

		protected bool HasTransaction(out IFileTransaction transaction)
		{
			transaction = null;

			if (!_UseTransactions) return false;

			if (_TxManager != null && _TxManager.CurrentTransaction != null)
			{
				foreach (var resource in _TxManager.CurrentTransaction.Resources())
				{
					if (!(resource is FileResourceAdapter)) continue;

					transaction = (resource as FileResourceAdapter).Transaction;
					return true;
				}

				if (!_OnlyJoinExisting)
				{
					transaction = new FileTransaction("Autocreated File Transaction");
					_TxManager.CurrentTransaction.Enlist(new FileResourceAdapter(transaction));
					return true;
				}
			}
			
			return false;
		}

		protected internal bool IsInAllowedDir(string path)
		{
			if (_AllowOutsideSpecifiedFolder) return true;

			var tentativePath = PathInfo.Parse(path);

			// if the given non-root is empty, we are looking at a relative path
			if (string.IsNullOrEmpty(tentativePath.Root)) return true;

			var specifiedPath = PathInfo.Parse(_SpecifiedFolder);

			// they must be on the same drive.
			if (!string.IsNullOrEmpty(tentativePath.DriveLetter)
			    && specifiedPath.DriveLetter != tentativePath.DriveLetter)
				return false;

			// we do not allow access to directories outside of the specified directory.
			return specifiedPath.IsParentOf(tentativePath);
		}

		protected void AssertAllowed(string path)
		{
			if (_AllowOutsideSpecifiedFolder) return;

			var fullPath = Path.GetFullPath(path);

			if (!IsInAllowedDir(fullPath))
				throw new UnauthorizedAccessException(
					string.Format("Authorization required for handling path \"{0}\" (passed as \"{1}\")", fullPath, path));
		}
	}
}