#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using Castle.Facilities.Transactions.Testing;
using Castle.Facilities.Transactions.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Facilities.Transactions;
using Castle.Facilities.Transactions.Activities;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Facilities.Transactions.Tests
{
	using TestClasses;

	[Ignore("For v3.1: Implement retry policies are nicer to test with e.g. NHibernate integration parts.")]
	public class RetryPolicies_Transactions
	{
		private IWindsorContainer _Container;
		private ITransactionManager _TransactionManager;

		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer().Register(

				Component.For<MyService2>(),

				Component.For<ITransactionManager>()
					.ImplementedBy<TransactionManager>()
					.Named("transaction.manager")
					.LifeStyle.Singleton,

				// the activity manager shouldn't have the same lifestyle as TransactionInterceptor, as it
				// calls a static .Net/Mono framework method, and it's the responsibility of
				// that framework method to keep track of the call context.
				Component.For<IActivityManager>()
					.ImplementedBy<CallContextActivityManager>()
					.LifeStyle.Singleton);

			_Container.Register();

			_TransactionManager = _Container.Resolve<ITransactionManager>();
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Release(_TransactionManager);
			_Container.Dispose();
		}

		//// something like: 
		//// http://philbolduc.blogspot.com/2010/03/retryable-actions-in-c.html

		//[Test]
		//public void retrying_twice_on_timeout()
		//{
		//    // on app-start
		//    var counter = 0;
		//    _TransactionManager.AddRetryPolicy("timeouts", e => e is TimeoutException && ++counter <= 2);

		//    using (var tx = _TransactionManager.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
		//    using (var s = new ResolveScope<IMyService>(_Container))
		//    {
		//        // in action
		//        s.Service.VerifyInAmbient(() =>
		//        {
		//            if (_TransactionManager.CurrentTransaction
		//                .Do(x => x.FailedPolicy)
		//                .Do(x => x.Failures < 2)
		//                .OrThrow(() => new Exception("Test failure; maybe doesn't have value!")))
		//                throw new TimeoutException("database not responding in a timely manner");
		//        });
		//    }
		//}
	}
}