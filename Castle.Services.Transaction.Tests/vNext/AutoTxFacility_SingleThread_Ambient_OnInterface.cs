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
using System.Transactions;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction;
using Castle.Windsor;
using log4net.Config;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class AutoTxFacility_SingleThread_Ambient_OnInterface
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

		[Test]
		public void Automatically_Starts_CommitableTransaction()
		{
			using (var scope = new ResolveScope<IMyService>(_Container))
				scope.Service.VerifyInAmbient();
		}

		[Test]
		public void IsDisposed_OnException_And_ActiveDuring_MethodCall()
		{
			using (var txM = new ResolveScope<ITxManager>(_Container))
			{
				System.Transactions.Transaction ambient = null;
				vNextTransaction.ITransaction ourTx = null;

				try
				{
					using (var scope = new ResolveScope<IMyService>(_Container))
						scope.Service.VerifyInAmbient(() =>
						{
							ambient = System.Transactions.Transaction.Current;
							ourTx = txM.Service.CurrentTransaction.Value;
							Assert.That(ourTx.State, Is.EqualTo(TransactionState.Active));
							throw new ApplicationException("should trigger rollback");
						});
				}
				catch (ApplicationException) { }

				Assert.That(ourTx.State, Is.EqualTo(TransactionState.Diposed));
			}
		}

		[Test]
		public void RecursiveTransactions_Inner_Should_Be_DependentTransaction()
		{
			using (var txM = new ResolveScope<ITxManager>(_Container))
			using (var scope = new ResolveScope<IMyService>(_Container))
				scope.Service.VerifyInAmbient(() =>
				{
					Assert.That(txM.Service.CurrentTransaction.Value.Inner.TransactionInformation.Status ==
								System.Transactions.TransactionStatus.Active);

					Assert.That(txM.Service.CurrentTransaction.Value.Inner, Is.InstanceOf<CommittableTransaction>());

					scope.Service.VerifyInAmbient(() => Assert.That(txM.Service.CurrentTransaction.Value.Inner, Is.InstanceOf<DependentTransaction>()));
				});
		}

		[Test]
		public void Method_Can_RollbackItself()
		{
			try
			{
				using (var txM = new ResolveScope<ITxManager>(_Container))
				using (var scope = new ResolveScope<IMyService>(_Container))
				{
					scope.Service.VerifyInAmbient(() => txM.Service.CurrentTransaction.Value.Rollback());
				}
				Assert.Fail("not rolled back");
			}
			catch (TransactionAbortedException)
			{
			}
		}

		[Test]
		public void Inline_Rollback_ResourceGetsRolledBack()
		{
			var resource = new ThrowingResource(false);

			try
			{
				using (var txM = new ResolveScope<ITxManager>(_Container))
				using (var scope = new ResolveScope<IMyService>(_Container))
				{
					scope.Service.VerifyInAmbient(() =>
					{
					    txM.Service.CurrentTransaction.Value.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
					    txM.Service.CurrentTransaction.Value.Rollback();
					});
				}
				Assert.Fail("the transaction threw, so it should have been aborted");
			}
			catch (TransactionAbortedException)
			{
				Assert.That(resource.WasRolledBack);
			}
		}
	}
}