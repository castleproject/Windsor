namespace Castle.Facilities.LightweighFactory.Tests
{
	using System;

	public class UsesBarDelegate
	{
		private readonly Func<string, string, Bar> barFactory;

		public UsesBarDelegate(Func<string,string,Bar> barFactory)
		{
			this.barFactory = barFactory;
		}
		public Bar GetBar(string name, string description)
		{
			return barFactory(name, description);
		}
	}
}