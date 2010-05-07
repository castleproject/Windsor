using Castle.Services.Transaction;
using Castle.Services.Transaction.IO;

namespace Castle.Facilities.AutoTx.Tests
{
	public interface ISomething
	{
		void A(ITransaction tx);
		void B(ITransaction tx);
		IDirectoryAdapter Da { get; }
		IFileAdapter Fa { get; }
	}
}