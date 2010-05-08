namespace Castle.Facilities.LightweighFactory.Tests
{
	using System;

	public class UsesFooDelegate
	{
		private readonly Func<int, Foo> myFooFactory;
		private int counter;

		public UsesFooDelegate(Func<int, Foo> myFooFactory)
		{
			this.myFooFactory = myFooFactory;
			counter = 0;
		}

		public Foo GetFoo()
		{
			return myFooFactory(++counter);
		}
	}
}