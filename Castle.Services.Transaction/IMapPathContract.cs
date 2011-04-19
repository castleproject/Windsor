using System;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction
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