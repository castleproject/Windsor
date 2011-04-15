using System.Diagnostics.Contracts;
using NHibernate;

namespace Castle.Facilities.NHibernate
{
	/// <summary>
	/// Session manager interface. This denotes the ISession factory. The default
	/// session lifestyle is per-transaction, so call OpenSession within a transaction!
	/// </summary>
	[ContractClass(typeof(ISessionManagerContract))]
	public interface ISessionManager
	{
		ISession OpenSession();
	}
}