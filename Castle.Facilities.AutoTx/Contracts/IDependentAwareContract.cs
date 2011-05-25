using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Castle.Facilities.Transactions.Internal;

namespace Castle.Facilities.Transactions.Contracts
{
	[ContractClassFor(typeof(IDependentAware))]
	internal abstract class IDependentAwareContract : IDependentAware
	{
		public void RegisterDependent(Task task)
		{
			Contract.Requires(task != null, "only register non-null tasks");
		}
	}
}