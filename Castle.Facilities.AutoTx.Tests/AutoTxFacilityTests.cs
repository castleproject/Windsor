using System;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Transaction;
using Castle.Services.Transaction.IO;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class AutoTxFacilityTests
	{
		[Test]
		public void Container_InjectsTransactions_IfTransactionInjectAttribute_is_set()
		{
			WindsorContainer c = new WindsorContainer(new DefaultConfigurationStore());

			c.AddFacility("transactionmanagement", new TransactionFacility());
			c.AddComponent("transactionmanager", typeof(ITransactionManager), typeof(MockTransactionManager));
			c.AddComponent("AClass", typeof(ISomething), typeof(AClass));

			var something = c.Resolve<ISomething>();

			Assert.That(something, Is.Not.Null);
			Assert.That(something.Da, Is.Not.Null);
			Assert.That(something.Fa, Is.Not.Null);

			something.A(null);
			something.B(null);
		}

		[Test]
		public void TestChildTransactions()
		{
			var container = new WindsorContainer();

			container.AddFacility("transactionmanagement", new TransactionFacility());
			container.AddComponent("transactionmanager", typeof(ITransactionManager), typeof(MockTransactionManager));

			container.AddComponent("mycomp", typeof(CustomerService));
			container.AddComponent("delegatecomp", typeof(ProxyService));

			var serv = container.Resolve<ProxyService>("delegatecomp");

			serv.DelegateInsert("John", "Home Address");

			var transactionManager = (MockTransactionManager) container["transactionmanager"];

			Assert.AreEqual(2, transactionManager.TransactionCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);
		}

		[Test]
		public void FileAndDirectoryAdapterResolveManager()
		{
			var container = new WindsorContainer();

			container.AddFacility("transactionmanagement", new TransactionFacility());
			container.AddComponent("transactionmanager", typeof(ITransactionManager), typeof(MockTransactionManager));

			container.AddComponent("mycomp", typeof(CustomerService));
			container.AddComponent("delegatecomp", typeof(ProxyService));

			var fa = (FileAdapter)container.Resolve<IFileAdapter>();
			Assert.That(fa.TxManager, Is.Not.Null);

			var da = (DirectoryAdapter)container.Resolve<IDirectoryAdapter>();
			Assert.That(da.TxManager, Is.Not.Null);
		}

		[Test]
		public void FileAndDirectoryAdapterResolveManager_OtherWayAround()
		{
			var container = new WindsorContainer();

			// these lines have been permuted
			container.AddComponent("transactionmanager", typeof(ITransactionManager), typeof(MockTransactionManager)); 
			container.AddFacility("transactionmanagement", new TransactionFacility());

			container.AddComponent("mycomp", typeof(CustomerService));
			container.AddComponent("delegatecomp", typeof(ProxyService));

			var fa = (FileAdapter)container.Resolve<IFileAdapter>();
			Assert.That(fa.TxManager, Is.Not.Null);

			var da = (DirectoryAdapter)container.Resolve<IDirectoryAdapter>();
			Assert.That(da.TxManager, Is.Not.Null);
		}

	}
	[Transactional]
	public class ProxyService
	{
		private readonly CustomerService customerService;
		public ProxyService(CustomerService customerService)
		{
			this.customerService = customerService;
		}

		[Transaction(TransactionMode.Requires)]
		public virtual void DelegateInsert(string name, string
address)
		{
			customerService.Insert(name, address);
		}
	}

}