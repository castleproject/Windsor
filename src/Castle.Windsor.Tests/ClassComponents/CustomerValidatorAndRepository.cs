namespace Castle.MicroKernel.Tests.ClassComponents
{
	public class CustomerValidatorAndRepository : IValidator<ICustomer>, IRepository<ICustomer>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="customer"></param>
		/// <returns></returns>
		public bool IsValid(ICustomer customer)
		{
			return true;
		}

		public ICustomer Find()
		{
			return new CustomerImpl();
		}
	}
}