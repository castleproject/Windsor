using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Tests
{
	public interface ISomething
	{
		void A(ITransaction tx);
		void B(ITransaction tx);
	}
}