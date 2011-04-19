using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net.Config;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class Multiple_Threads_DependentTransactionWithParent
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}
	}
}