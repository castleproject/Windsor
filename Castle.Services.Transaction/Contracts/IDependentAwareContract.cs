using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Castle.Services.Transaction.Internal;

namespace Castle.Services.Transaction.Contracts
{
	[ContractClassFor(typeof(IDependentAware))]
	public class IDependentAwareContract : IDependentAware
	{
		public void RegisterDependent(Task task)
		{
			Contract.Requires(task != null, "only register non-null tasks");
		}
	}
}