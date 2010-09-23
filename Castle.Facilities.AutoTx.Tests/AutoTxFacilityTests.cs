#region License
//  Copyright 2004-2010 Castle Project - http:www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http:www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

namespace Castle.Facilities.AutoTx.Tests
{
	using MicroKernel.Registration;
	using MicroKernel.SubSystems.Configuration;
	using NUnit.Framework;
	using Services.Transaction;
	using Services.Transaction.IO;
	using Windsor;

	public class AutoTxFacilityTests
	{
		[Test]
		public void Container_InjectsTransactions_IfTransactionInjectAttribute_is_set()
		{
			WindsorContainer c = new WindsorContainer(new DefaultConfigurationStore());

			c.AddFacility("transactionmanagement", new TransactionFacility());
			c.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));
			c.Register(Component.For<ISomething>().ImplementedBy<AClass>().Named("AClass"));

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
			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			container.Register(Component.For<ProxyService>().Named("delegatecomp"));

			var serv = container.Resolve<ProxyService>("delegatecomp");

			serv.DelegateInsert("John", "Home Address");

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(2, transactionManager.TransactionCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);
		}

		[Test]
		public void FileAndDirectoryAdapterResolveManager()
		{
			var container = new WindsorContainer();

			container.AddFacility("transactionmanagement", new TransactionFacility());
			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			container.Register(Component.For<ProxyService>().Named("delegatecomp"));

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
			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager")); 
			container.AddFacility("transactionmanagement", new TransactionFacility());

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			container.Register(Component.For<ProxyService>().Named("delegatecomp"));

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