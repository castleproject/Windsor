#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion

namespace Castle.Facilities.NHibernateIntegration.Tests.Transactions
{
	using System;
	using AutoTx;
	using MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class TransactionWithTwoDatabasesTestCase : AbstractNHibernateTestCase
	{
		protected override string ConfigurationFile
		{
			get { return "Transactions/TwoDatabaseConfiguration.xml"; }
		}

		protected override void ConfigureContainer()
		{
			container.AddFacility("transactions", new TransactionFacility());

			container.Register(Component.For<RootService>().Named("root"));
			container.Register(Component.For<FirstDao>().Named("myfirstdao"));
			container.Register(Component.For<SecondDao>().Named("myseconddao"));
			container.Register(Component.For<OrderDao>().Named("myorderdao"));
		}

		[Test]
		public void SuccessfulSituationWithTwoDatabases()
		{
			RootService service = container.Resolve<RootService>();
			OrderDao orderDao = container.Resolve<OrderDao>("myorderdao");

			service.DoTwoDBOperation_Create(false);

			Array blogs = service.FindAll(typeof (Blog));
			Array blogitems = service.FindAll(typeof (BlogItem));
			Array orders = orderDao.FindAll(typeof (Order));

			Assert.IsNotNull(blogs);
			Assert.IsNotNull(blogitems);
			Assert.IsNotNull(orders);
			Assert.AreEqual(1, blogs.Length);
			Assert.AreEqual(1, blogitems.Length);
			Assert.AreEqual(1, orders.Length);
		}

		[Test]
		public void ExceptionOnEndWithTwoDatabases()
		{
			RootService service = container.Resolve<RootService>();
			OrderDao orderDao = container.Resolve<OrderDao>("myorderdao");

			try
			{
				service.DoTwoDBOperation_Create(true);
			}
			catch (InvalidOperationException)
			{
				// Expected
			}

			Array blogs = service.FindAll(typeof (Blog));
			Array blogitems = service.FindAll(typeof (BlogItem));
			Array orders = orderDao.FindAll(typeof (Order));

			Assert.IsNotNull(blogs);
			Assert.IsNotNull(blogitems);
			Assert.IsNotNull(orders);
			Assert.AreEqual(0, blogs.Length);
			Assert.AreEqual(0, blogitems.Length);
			Assert.AreEqual(0, orders.Length);
		}

		[Test]
		public void SuccessfulSituationWithTwoDatabasesStateless()
		{
			RootService service = container.Resolve<RootService>();
			OrderDao orderDao = container.Resolve<OrderDao>("myorderdao");

			service.DoTwoDBOperation_Create_Stateless(false);

			Array blogs = service.FindAllStateless(typeof(Blog));
			Array blogitems = service.FindAllStateless(typeof(BlogItem));
			Array orders = orderDao.FindAllStateless(typeof(Order));

			Assert.IsNotNull(blogs);
			Assert.IsNotNull(blogitems);
			Assert.IsNotNull(orders);
			Assert.AreEqual(1, blogs.Length);
			Assert.AreEqual(1, blogitems.Length);
			Assert.AreEqual(1, orders.Length);
		}

		[Test]
		public void ExceptionOnEndWithTwoDatabasesStateless()
		{
			RootService service = container.Resolve<RootService>();
			OrderDao orderDao = container.Resolve<OrderDao>("myorderdao");

			try
			{
				service.DoTwoDBOperation_Create_Stateless(true);
			}
			catch (InvalidOperationException)
			{
				// Expected
			}

			Array blogs = service.FindAllStateless(typeof(Blog));
			Array blogitems = service.FindAllStateless(typeof(BlogItem));
			Array orders = orderDao.FindAllStateless(typeof(Order));

			Assert.IsNotNull(blogs);
			Assert.IsNotNull(blogitems);
			Assert.IsNotNull(orders);
			Assert.AreEqual(0, blogs.Length);
			Assert.AreEqual(0, blogitems.Length);
			Assert.AreEqual(0, orders.Length);
		}
	}
}