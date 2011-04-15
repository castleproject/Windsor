using System;
using System.Diagnostics.Contracts;
using NHibernate;

namespace Castle.Facilities.NHibernate
{
	[ContractClassFor(typeof (ISessionManager))]
	internal abstract class ISessionManagerContract : ISessionManager
	{
		ISession ISessionManager.OpenSession()
		{
			Contract.Ensures(Contract.Result<ISession>() != null);
			throw new NotImplementedException();
		}
	}
}