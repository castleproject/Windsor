using System.Threading;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.Framework
{
	public class STATestRuntime
	{
		[Test, Description("As we are working on the same folders, we don't want to run the tests concurrently.")]
		public void CheckSTA()
		{
			ApartmentState aptState = Thread.CurrentThread.GetApartmentState();
			Assert.IsTrue(aptState == ApartmentState.STA);
		}
	}
}