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

using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Transaction;
using Castle.Services.Transaction.IO;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	[TestFixture]
	public class AutoTxFacilityTests
	{
		[Test]
		public void Container_InjectsTransactions_IfTransactionInjectAttribute_is_set()
		{
			var c = GetContainer();
			
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
			CreateContainer = () => new WindsorContainer()
				.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>()
				.Forward(typeof(MockTransactionManager)));

			var container = GetContainer();

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			container.Register(Component.For<ProxyService>().Named("delegatecomp"));

			var serv = container.Resolve<ProxyService>("delegatecomp");

			serv.DelegateInsert("John", "Home Address");

			var transactionManager = container.Resolve<MockTransactionManager>();

			Assert.AreEqual(2, transactionManager.TransactionCount);
			Assert.AreEqual(0, transactionManager.RolledBackCount);
		}

		private Func<IWindsorContainer> CreateContainer = () => new WindsorContainer();

		private IWindsorContainer GetContainer()
		{
			var container = CreateContainer();
			container.AddFacility("transactionmanagement", new TransactionFacility());
			return container;
		}

		[Test]
		public void TestReadonlyTransactions()
		{
			var container = GetContainer();
			container.Register(Component.For<CustomerService>().Named("mycomp"));
			
			var service = container.Resolve<CustomerService>();
			service.DoSomethingNotMarkedAsReadOnly();
			service.DoSomethingReadOnly();
		}

		[Test]
		public void FileAndDirectoryAdapterResolveManager()
		{
			var container = GetContainer();

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			container.Register(Component.For<ProxyService>().Named("delegatecomp"));

			var fa = (FileAdapter) container.Resolve<IFileAdapter>();
			Assert.That(fa.TxManager, Is.Not.Null);

			var da = (DirectoryAdapter) container.Resolve<IDirectoryAdapter>();
			Assert.That(da.TxManager, Is.Not.Null);
		}

		[Test]
		public void FileAndDirectoryAdapterResolveManager_OtherWayAround()
		{
			var container = new WindsorContainer();

			// when we register the mock transaction manager
			container.Register(Component.For<ITransactionManager>().ImplementedBy<MockTransactionManager>().Named("transactionmanager"));
			container.AddFacility("transactionmanagement", new TransactionFacility());

			container.Register(Component.For<CustomerService>().Named("mycomp"));
			container.Register(Component.For<ProxyService>().Named("delegatecomp"));

			var fa = (FileAdapter) container.Resolve<IFileAdapter>();
			Assert.That(fa.TxManager, Is.Not.Null);

			var da = (DirectoryAdapter) container.Resolve<IDirectoryAdapter>();
			Assert.That(da.TxManager, Is.Not.Null);
		}
	}
}