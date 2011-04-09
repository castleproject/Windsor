using System.Linq;
using Castle.Services.vNextTransaction;
using NUnit.Framework;
using Castle.Services.Transaction.Tests.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class InfrastructureTests
	{
		[Test]
		public void CanGetTxClassMetaInfo()
		{
			var meta = TxClassMetaInfoStore.GetMetaFromTypeInner(typeof (MyService));
			meta.ShouldPass("MyService has Transaction attributes")
				.ShouldBe(m => m.TransactionalMethods.Count() == 2, "there are two methods");
		}
	}
}