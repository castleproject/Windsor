namespace Castle.Facilities.LightweighFactory.Tests
{
	public class Foo
	{
		private readonly int number;

		public Foo(int number)
		{
			this.number = number;
		}

		public int Number
		{
			get { return number; }
		}
	}
}