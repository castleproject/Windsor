using NUnit.Framework;

namespace Castle.Windsor.Tests
{
    using Castle.MicroKernel.Registration;

    [TestFixture]
	public class RegisteringTwoServicesForSameType
	{
		public interface IService{}
		public class Srv1 : IService{}
		public class Srv2 : IService { }


		[Test]
		public void ResolvingComponentIsDoneOnFirstComeBasis()
		{
            IWindsorContainer windsor = new WindsorContainer();
			windsor.Register(Component.For<IService>().ImplementedBy<Srv1>().Named("1"));
			windsor.Register(Component.For<IService>().ImplementedBy<Srv1>().Named("2"));

			Assert.IsInstanceOf<Srv1>(windsor.Resolve<IService>());
		}

		[Test]
		public void ResolvingComponentIsDoneOnFirstComeBasisWhenNamesAreNotOrdered()
		{
            IWindsorContainer windsor = new WindsorContainer();
            windsor.Register(Component.For<IService>().ImplementedBy<Srv1>().Named("3"));
            windsor.Register(Component.For<IService>().ImplementedBy<Srv1>().Named("2"));

			Assert.IsInstanceOf<Srv1>(windsor.Resolve<IService>());
		}
	}
}