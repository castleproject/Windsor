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
using Castle.Facilities.AutoTx.Testing;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Transactions;
using Castle.Windsor;
using log4net.Config;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class SingleThread_Ambient_OnInterface
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<MyService>());
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void Automatically_Starts_CommitableTransaction()
		{
			using (var scope = new ResolveScope<MyService>(_Container))
				scope.Service.VerifyInAmbient();
		}

		[Test]
		public void IsDisposed_OnException_And_ActiveDuring_MethodCall()
		{
			using (var txM = new ResolveScope<ITransactionManager>(_Container))
			{
				System.Transactions.Transaction ambient = null;
				ITransaction ourTx = null;

				try
				{
					using (var scope = new ResolveScope<MyService>(_Container))
						scope.Service.VerifyInAmbient(() =>
						{
							ambient = System.Transactions.Transaction.Current;
							ourTx = txM.Service.CurrentTransaction.Value;
							Assert.That(ourTx.State, Is.EqualTo(TransactionState.Active));
							throw new ApplicationException("should trigger rollback");
						});
				}
				catch (ApplicationException) { }

				Assert.That(ourTx.State, Is.EqualTo(TransactionState.Disposed));
			}
		}

		[Test]
		public void RecursiveTransactions_Inner_Should_Be_DependentTransaction()
		{
			using (var txM = new ResolveScope<ITransactionManager>(_Container))
			using (var scope = new ResolveScope<MyService>(_Container))
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
			TransactionState state = TransactionState.Default;
			using (var txM = new ResolveScope<ITransactionManager>(_Container))
			using (var scope = new ResolveScope<MyService>(_Container))
			{
				scope.Service.VerifyInAmbient(() =>
				{
					txM.Service.CurrentTransaction.Value.Rollback();
					state = txM.Service.CurrentTransaction.Value.State;
				});
			}
			Assert.That(state, Is.EqualTo(TransactionState.Aborted));
		}

		[Test]
		public void Inline_Rollback_ResourceGetsRolledBack()
		{
			var resource = new ThrowingResource(false);

			using (var txM = new ResolveScope<ITransactionManager>(_Container))
			using (var scope = new ResolveScope<MyService>(_Container))
			{
				scope.Service.VerifyInAmbient(() =>
				{
					txM.Service.CurrentTransaction.Value.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
					txM.Service.CurrentTransaction.Value.Rollback();
				});
			}

			Assert.That(resource.WasRolledBack);
		}
	}
}