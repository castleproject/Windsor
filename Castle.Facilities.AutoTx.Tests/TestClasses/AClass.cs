using Castle.Services.Transaction;
using Castle.Services.Transaction.IO;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	[Transactional]
	public class AClass : ISomething
	{
		private readonly IDirectoryAdapter _Da;
		private readonly IFileAdapter _Fa;

		public AClass(IDirectoryAdapter da, IFileAdapter fa)
		{
			_Da = da;
			_Fa = fa;
		}

		public IDirectoryAdapter Da
		{
			get { return _Da; }
		}

		public IFileAdapter Fa
		{
			get { return _Fa; }
		}

		[Transaction]
		public void A(ITransaction tx)
		{
			Assert.That(tx, Is.Null);
		}

		[Transaction, InjectTransaction]
		public void B(ITransaction tx)
		{
			Assert.That(tx, Is.Not.Null);
		}
	}
}