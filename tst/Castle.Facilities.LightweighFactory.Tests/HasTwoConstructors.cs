namespace Castle.Facilities.LightweighFactory.Tests
{
	public class HasTwoConstructors
	{
		private readonly Baz baz;
		private readonly string name;

		public Baz Baz
		{
			get { return baz; }
		}

		public string Name
		{
			get { return name; }
		}

		public HasTwoConstructors(Baz baz)
		{
			this.baz = baz;
		}

		public HasTwoConstructors(Baz baz, string name)
		{
			this.baz = baz;
			this.name = name;
		}

	}
}