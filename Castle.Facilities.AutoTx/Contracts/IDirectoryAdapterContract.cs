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

namespace Castle.Services.Transaction.Contracts
{
	[ContractClassFor(typeof (IDirectoryAdapter))]
	internal abstract class IDirectoryAdapterContract : IDirectoryAdapter
	{
		bool IDirectoryAdapter.Create(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		bool IDirectoryAdapter.Exists(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		void IDirectoryAdapter.Delete(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		bool IDirectoryAdapter.Delete(string path, bool recursively)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		string IDirectoryAdapter.GetFullPath(string relativeDir)
		{
			Contract.Requires(!string.IsNullOrEmpty(relativeDir));
			throw new NotImplementedException();
		}

		string IDirectoryAdapter.MapPath(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path));
			throw new NotImplementedException();
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalPath));
			Contract.Requires(!string.IsNullOrEmpty(newPath));
			throw new NotImplementedException();
		}

		void IDirectoryAdapter.Move(string originalPath, string newPath, bool overwrite)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalPath));
			Contract.Requires(!string.IsNullOrEmpty(newPath));
			throw new NotImplementedException();
		}
	}
}