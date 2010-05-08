namespace Castle.Facilities.LightweighFactory.Tests
{
	public class HasOnlyOneArgMatchingDelegatesParameter
	{
		private readonly string arg1;
		private readonly string name;

		public HasOnlyOneArgMatchingDelegatesParameter(string arg1, string name)
		{
			this.arg1 = arg1;
			this.name = name;
		}

		public string Arg1
		{
			get { return arg1; }
		}

		public string Name
		{
			get { return name; }
		}
	}
}