namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components
{
	using System;

	using Castle.Core;

	public class A_Facilities_Issue_111 : IA_Facilities_Issue_111, IStartable
	{
		public A_Facilities_Issue_111(IB_Facilities_Issue_111[] ibs)
		{
			this.ibs = ibs;
		}

		IB_Facilities_Issue_111[] ibs;

		public void Method()
		{
			Console.WriteLine("A: Method");
		}

		public void Start()
		{
			Console.WriteLine("Started A");
		}

		public void Stop()
		{
			Console.WriteLine("Stopped A");
		}
	}
}
