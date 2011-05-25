namespace Castle.Facilities.Transactions.Tests
{
	using NUnit.Framework;

	public class TransactionManager_IgnoreDependents
	{
		[Test, Ignore("For Beta")]
		public void If_Parent_Completes_BeforeChild_ButChildThrows_NoFinalizerShouldThrowException_FromTask()
		{
			// see the multi-threaded tests on AutoTx project
		}
	}
}