using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Tests
{
	[Transactional]
	public class ProxyService
	{
		private readonly CustomerService customerService;

		public ProxyService(CustomerService customerService)
		{
			this.customerService = customerService;
		}

		[Transaction(TransactionMode.Requires)]
		public virtual void DelegateInsert(string name, string
		                                                	address)
		{
			customerService.Insert(name, address);
		}
	}
}