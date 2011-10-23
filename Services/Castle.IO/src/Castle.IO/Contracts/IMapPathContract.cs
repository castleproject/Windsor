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

namespace Castle.IO.Contracts
{
	[ContractClassFor(typeof (IMapPath))]
	internal abstract class IMapPathContract : IMapPath
	{
		public string MapPath(string path)
		{
			Contract.Requires(!string.IsNullOrEmpty(path), "path must be non null and not empty");
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
			throw new NotImplementedException();
		}
	}
}