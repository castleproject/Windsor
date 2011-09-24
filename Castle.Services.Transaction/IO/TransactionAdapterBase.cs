#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Castle.Services.Transaction.IO
{
	// http://social.msdn.microsoft.com/Forums/en-CA/windowstransactionsprogramming/thread/ab4946d9-b634-4156-9296-78554d41d84a
	// http://www.pluralsight-training.net/community/blogs/jimjohn/archive/2006/09/01/36863.aspx

	///<summary>
	///	Adapter base class for the file and directory adapters.
	///</summary>
	public abstract class TransactionAdapterBase
	{
		private readonly bool _AllowOutsideSpecifiedFolder;
		private readonly string _SpecifiedFolder;
		private ITransactionManager _TransactionManager;
		private bool _UseTransactions = true;
		private bool _OnlyJoinExisting;
        private ILogger _Logger = NullLogger.Instance;

        public ILogger Logger
        {
            get { return _Logger; }
            set { _Logger = value; }
        }

		protected TransactionAdapterBase(bool constrainToSpecifiedDir,
		                        string specifiedDir)
		{
			Contract.Requires(!constrainToSpecifiedDir || !string.IsNullOrEmpty(specifiedDir));
			_AllowOutsideSpecifiedFolder = !constrainToSpecifiedDir;
			_SpecifiedFolder = specifiedDir;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_AllowOutsideSpecifiedFolder || _SpecifiedFolder != null);
			Contract.Invariant(!_AllowOutsideSpecifiedFolder || _SpecifiedFolder == null);
		}

		/// <summary>
		/// 	Gets the transaction manager, if there is one, or sets it.
		/// </summary>
		public ITransactionManager TransactionManager
		{
			get { return _TransactionManager; }
			set { _TransactionManager = value; }
		}

		///<summary>
		///	Gets/sets whether to use transactions.
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

		protected bool HasTransaction(out ITransaction transaction)
		{
			transaction = null;

			if (!_UseTransactions) return false;
			return _TransactionManager != null && _TransactionManager.CurrentTransaction.HasValue;
			//if (_TransactionManager != null && _TransactionManager.CurrentTransaction.HasValue)
			//{
			//    foreach (var resource in _TransactionManager.CurrentTransaction.Resources())
			//    {
			//        if (!(resource is FileResourceAdapter)) continue;

			//        transaction = (resource as FileResourceAdapter).Transaction;
			//        return true;
			//    }

			//    if (!_OnlyJoinExisting)
			//    {
			//        throw new NotImplementedException();
			//        transaction = new FileTransaction("Autocreated File Transaction");
			//        _TransactionManager.CurrentTransaction.Enlist(new FileResourceAdapter(transaction));
			//        return true;
			//    }
			//}

			//return false;
		}

		protected internal bool IsInAllowedDir(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));

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
			Contract.Requires(!string.IsNullOrEmpty(path));

			if (_AllowOutsideSpecifiedFolder) return;

			var fullPath = Path.GetFullPath(path);

			if (!IsInAllowedDir(fullPath))
				throw new UnauthorizedAccessException(
					string.Format("Authorization required for handling path \"{0}\" (passed as \"{1}\")", fullPath, path));
		}
	}
}