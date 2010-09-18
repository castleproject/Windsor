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
	using System;
	using MicroKernel.Registration;
	using MicroKernel.SubSystems.Configuration;
	using NUnit.Framework;
	using Services.Transaction;
	using Windsor;

	[TestFixture]
	public class FacilityBasicTests
	{
		[Test]
		public void TestReportedBug()
		{
			WindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());

			container.AddFacility("transactionmanagement", new TransactionFacility());

			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));
			container.Register(Component.For<SubTransactionalComp>().Named("comp"));

			SubTransactionalComp service = container.Resolve<SubTransactionalComp>("comp");

			service.BaseMethod();

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(1, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);
		}

		[Test]
		public void TestBasicOperations()
		{
			WindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());

			container.AddFacility("transactionmanagement", new TransactionFacility());

			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));

			container.Register(Component.For<CustomerService>().Named("services.customer"));

			CustomerService service = container.Resolve<CustomerService>("services.customer");

			service.Insert("TestCustomer", "Rua P Leite, 33");

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(1, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);

			try
			{
				service.Delete(1);
			}
			catch (Exception)
			{
				// Expected
			}

			Assert.AreEqual(2, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(1, transactionManager.RolledBackCount);
		}

		[Test]
		public void TestBasicOperationsWithInterfaceService()
		{
			WindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());

			container.AddFacility("transactionmanagement", new TransactionFacility());
			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));
			container.Register(Component.For<ICustomerService>().ImplementedBy<AnotherCustomerService>().Named("services.customer"));

			ICustomerService service = container.Resolve<ICustomerService>("services.customer");

			service.Insert("TestCustomer", "Rua P Leite, 33");

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(1, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);

			try
			{
				service.Delete(1);
			}
			catch (Exception)
			{
				// Expected
			}

			Assert.AreEqual(2, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(1, transactionManager.RolledBackCount);
		}

		[Test]
		public void TestBasicOperationsWithGenericService()
		{
			WindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());

			container.AddFacility("transactionmanagement", new TransactionFacility());
			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));
			container.Register(Component.For(typeof(GenericService<>)).Named("generic.services"));

			GenericService<string> genericService = container.Resolve<GenericService<string>>();

			genericService.Foo();

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(1, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);

			try
			{
				genericService.Throw();
			}
			catch (Exception)
			{
				// Expected
			}

			Assert.AreEqual(2, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.CommittedCount);
			Assert.AreEqual(1, transactionManager.RolledBackCount);

			genericService.Bar<int>();
			
			Assert.AreEqual(3, transactionManager.TransactionCount);
			Assert.AreEqual(2, transactionManager.CommittedCount);
			Assert.AreEqual(1, transactionManager.RolledBackCount);

			try
			{
				genericService.Throw<float>();
			}
			catch
			{
				//exepected
			}

			Assert.AreEqual(4, transactionManager.TransactionCount);
			Assert.AreEqual(2, transactionManager.CommittedCount);
			Assert.AreEqual(2, transactionManager.RolledBackCount);
		}

		[Test]
		public void TestBasicOperationsWithConfigComponent()
		{
			WindsorContainer container = new WindsorContainer("HasConfiguration.xml");

			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));

			TransactionalComp1 comp1 = container.Resolve<TransactionalComp1>("mycomp");

			comp1.Create();

			comp1.Delete();

			comp1.Save();

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(3, transactionManager.TransactionCount);
			Assert.AreEqual(3, transactionManager.CommittedCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);
		}

		/// <summary>
		/// Tests the situation where the class uses
		/// ATM, but grab the transaction manager and rollbacks the 
		/// transaction manually
		/// </summary>
		[Test]
		public void RollBackExplicitOnClass()
		{
			WindsorContainer container = new WindsorContainer();

			container.AddFacility("transactionmanagement", new TransactionFacility());

			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			
			CustomerService serv = container.Resolve<CustomerService>("mycomp");

			serv.Update(1);

			MockTransactionManager transactionManager = container.Resolve<MockTransactionManager>("transactionmanager");

			Assert.AreEqual(1, transactionManager.TransactionCount);
			Assert.AreEqual(1, transactionManager.RolledBackCount);
			Assert.AreEqual(0, transactionManager.CommittedCount);
		}
	}
}
