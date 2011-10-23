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
using System.IO;

namespace Castle.IO.Contracts
{
	[ContractClassFor(typeof (IFileAdapter))]
	internal abstract class IFileAdapterContract : IFileAdapter
	{
		FileStream IFileAdapter.Create(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		bool IFileAdapter.Exists(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		void IFileAdapter.Delete(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		FileStream IFileAdapter.Open(string filePath, FileMode mode)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}

		FileStream IFileAdapter.OpenWrite(string path)
		{
			throw new NotImplementedException();
		}

		void IFileAdapter.Move(string originalFilePath, string newFilePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(originalFilePath));
			Contract.Requires(!string.IsNullOrEmpty(newFilePath));
			Contract.Requires(Path.GetFileName(originalFilePath).Length > 0);
			throw new NotImplementedException();
		}

		StreamWriter IFileAdapter.CreateText(string filePath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filePath));
			throw new NotImplementedException();
		}
	}
}