namespace Castle.Facilities.LightweighFactory.Tests
{
	public class Bar
	{
		private readonly Baz baz;
		private readonly string name;

		public Bar(Baz baz, string name)
		{
			this.baz = baz;
			this.name = name;
		}

		public string Description { get; set; }

		public string Name
		{
			get {
				return name;
			}
		}
	}
}