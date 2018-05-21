namespace CastleTests
{
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class CaseSensitiveRegistrationTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Registration_is_case_sensitive()
		{
			Container.Register(Component.For<ICaseSensitive>().ImplementedBy<CaseSensitive>());
			Container.Register(Component.For<ICaseSENSITIVE>().ImplementedBy<CaseSENSITIVE>());

			var caseSensitive = Container.Resolve<ICaseSensitive>();
			var caseSENSITIVE = Container.Resolve<ICaseSENSITIVE>();

			Assert.IsAssignableFrom<CaseSensitive>(caseSensitive);
			Assert.IsAssignableFrom<CaseSENSITIVE>(caseSENSITIVE);
		}
	}

	interface ICaseSensitive
	{
	}

	interface ICaseSENSITIVE
	{
	}

	class CaseSensitive : ICaseSensitive
	{
	}

	class CaseSENSITIVE : ICaseSENSITIVE
	{
	}
}