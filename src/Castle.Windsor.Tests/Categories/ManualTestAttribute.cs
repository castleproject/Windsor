namespace CastleTests.Categories
{
	using NUnit.Framework;

	public class ManualTestAttribute : CategoryAttribute
	{
		public ManualTestAttribute() : base("Manual")
		{
		}
	}
}