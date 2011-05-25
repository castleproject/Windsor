namespace Castle.Facilities.Transactions.Internal
{
	using System.Threading.Tasks;

	// reference: http://blogs.msdn.com/b/pfxteam/archive/2009/05/31/9674669.aspx
	public static class TaskExtensions
	{
		public static Task IgnoreExceptions(this Task task)
		{
			task.ContinueWith(c => { var ignored = c.Exception; },
				TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
			return task;
		}
	}
}