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

namespace Castle.Facilities.Transactions.IO
{
	using System;
	using System.Diagnostics.Contracts;
	using Core.Logging;

	/// <summary>
	/// 	Adapter which wraps the functionality in <see cref = "File" />
	/// 	together with native kernel transactions.
	/// </summary>
	public class DirectoryAdapter : TransactionAdapterBase, IDirectoryAdapter
	{
		private readonly IMapPath _PathFinder;

		private ILogger _Logger = NullLogger.Instance;

		/// <summary>
		/// 	Create a new DirectoryAdapter instance. C'tor.
		/// </summary>
		/// <param name = "pathFinder">The MapPath implementation.</param>
		/// <param name = "constrainToSpecifiedDir">Whether to ChJail the DirectoryAdapter.</param>
		/// <param name = "specifiedDir">The directory to constrain the adapter to.</param>
		public DirectoryAdapter(IMapPath pathFinder, bool constrainToSpecifiedDir, string specifiedDir)
			: base(constrainToSpecifiedDir, specifiedDir)
		{
			Contract.Requires(pathFinder != null);

			_PathFinder = pathFinder;
		}

		public ILogger Logger
		{
			get { return _Logger; }
			set { _Logger = value; }
		}

		bool IDirectoryAdapter.Create(string path)
		{
			AssertAllowed(path);

#if !MONO
			var maybe = CurrentTransaction();
		
			if (maybe.HasValue)
				return ((IDirectoryAdapter) maybe.Value).Create(path);
#endif

			if (((IDirectoryAdapter) this).Exists(path))
				return true;

			LongPathDirectory.Create(path);
			return true;
		}

		bool IDirectoryAdapter.Exists(string path)
		{
			AssertAllowed(path);

#if !MONO
			var mTx = CurrentTransaction();
			if (mTx.HasValue)
				return ((IDirectoryAdapter) mTx.Value).Exists(path);
#endif

			return LongPathDirectory.Exists(path);
		}

		void IDirectoryAdapter.Delete(string path)
		{
			AssertAllowed(path);
#if !MONO
			var mTx = CurrentTransaction();
			if (mTx.HasValue)
			{
				((IDirectoryAdapter) mTx.Value).Delete(path);
				return;
			}
#endif

			LongPathDirectory.Delete(path);
		}

		bool IDirectoryAdapter.Delete(string path, bool recursively)
		{
			AssertAllowed(path);
#if !MONO
			var mTx = CurrentTransaction();
			if (mTx.HasValue)
				return ((IDirectoryAdapter) mTx.Value).Delete(path, recursively);
#endif

			LongPathDirectory.Delete(path);
			return true;
		}

		string IDirectoryAdapter.GetFullPath(string path)
		{
			AssertAllowed(path);
#if !MONO
			var mTx = CurrentTransaction();
			if (mTx.HasValue)
				return ((IDirectoryAdapter) mTx.Value).GetFullPath(path);
#endif
			return LongPathCommon.NormalizeLongPath(path);
		}

		string IDirectoryAdapter.MapPath(string path)
		{
			return _PathFinder.MapPath(path);
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath)
		{
			((IDirectoryAdapter)this).Move(originalPath, newPath, false);
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath, bool overwrite)
		{
			AssertAllowed(originalPath);
			AssertAllowed(newPath);

#if !MONO
			var mTx = CurrentTransaction();
			if (mTx.HasValue)
			{
				((IDirectoryAdapter)mTx.Value).Move(originalPath, newPath);
				return;
			}
#endif

			throw new NotImplementedException();
			//LongPathFile.Move(originalPath, newPath);
		}
	}
}