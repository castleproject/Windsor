namespace Castle.Facilities.LightweighFactory.Tests
{
	public class Foo
	{
		private readonly int number;

		public Foo(int arg)
		{
			number = arg;
		}

		public int Number
		{
			get { return number; }
		}
	}
}