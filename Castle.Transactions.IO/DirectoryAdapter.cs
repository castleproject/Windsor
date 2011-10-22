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
using Castle.IO;
using Castle.IO.Internal;
using NLog;
using Path = Castle.IO.Path;

namespace Castle.Transactions.IO
{
	/// <summary>
	/// 	Adapter which wraps the functionality in <see cref = "IFile" />
	/// 	together with native kernel transactions.
	/// </summary>
	public class DirectoryAdapter : TransactionAdapterBase, IDirectoryAdapter
	{
		private readonly IMapPath pathFinder;
		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		public DirectoryAdapter() : this(new MapPathImpl(), false, null)
		{
		}

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

			logger.Debug("DirectoryAdapter created.");

			this.pathFinder = pathFinder;
		}

		bool IDirectoryAdapter.Create(string path)
		{
			AssertAllowed(path);
			try
			{
#if !MONO
				var maybe = CurrentTransaction();
		
				if (maybe.HasValue)
					return ((IDirectoryAdapter) maybe.Value).Create(path);
#endif

				if (((IDirectoryAdapter) this).Exists(path))
					return true;

				LongPathDirectory.Create(path);
			}
			catch (UnauthorizedAccessException e)
			{
				throw new UnauthorizedAccessException(string.Format("Access is denied for path '{0}', see inner exception for details.", 
					path), e);
			}
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

			// because of http://stackoverflow.com/questions/3764072/c-win32-how-to-wait-for-a-pending-delete-to-complete

			var target = Path.GetRandomFileName();
			LongPathDirectory.Move(path, target);
			LongPathDirectory.Delete(target, recursively);
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
			return pathFinder.MapPath(path);
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

			LongPathFile.Move(originalPath, newPath);
		}
	}
}