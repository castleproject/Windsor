// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.AutoTx.Tests
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Threading;

	using Castle.Facilities.AutoTx.Testing;
	using Castle.Facilities.FactorySupport;
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.Transactions;
	using Castle.Windsor;

	using NLog;

	using NUnit.Framework;

// ReSharper disable InconsistentNaming
	public class PerTransactionLifestyle_Releasing
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		[Test]
		public void ThrowsMissingTransactionException_NoAmbientTransaction()
		{
			// given
			var container = GetContainer();

			// when
			using (var scope = container.ResolveScope<Service>())
			{
				var ex = Assert.Throws<MissingTransactionException>(() => scope.Service.DoWork());
				Assert.That(ex.Message, Is.StringContaining("Castle.Facilities.AutoTx.Tests.IPerTxService"),
				            "The message from the exception needs to contain the component which IS A per-transaction component.");
			}
		}

		[Test]
		public void ThrowsMissingTransactionException_NoAmbientTransaction_DirectDependency()
		{
			// given
			var container = GetContainer();

			// when
			var ex = Assert.Throws<MissingTransactionException>(() =>
			{
				using (var scope = container.ResolveScope<ServiceWithDirectDep>())
					scope.Service.DoWork();
			});
			Assert.That(ex.Message, Is.StringContaining("Castle.Facilities.AutoTx.Tests.IPerTxService"),
			            "The message from the exception needs to contain the component which IS A per-transaction component.");
		}

		[Test]
		public void Same_Instances()
		{
			// given
			var container = GetContainer();

			// when
			using (var scope = container.ResolveScope<Service>())
			using (var manager = container.ResolveScope<ITransactionManager>())
			using (var tx = manager.Service.CreateTransaction().Value.Transaction)
			{
				var resolved = scope.Service.DoWork();
				var resolved2 = scope.Service.DoWork();
				Assert.That(resolved.Id, Is.EqualTo(resolved2.Id), "because they are resolved within the same transaction");
				tx.Rollback();
			}
		}

		[Test]
		public void Doesnt_Dispose_Twice()
		{
			// given
			var container = GetContainer();

			// exports from actions, to assert end-state
			IPerTxService serviceUsed;

			// when
			using (var scope = container.ResolveScope<Service>())
			using (var manager = container.ResolveScope<ITransactionManager>())
			using (var tx = manager.Service.CreateTransaction().Value.Transaction)
			{
				var resolved = scope.Service.DoWork();
				var resolved2 = scope.Service.DoWork();

				Assert.That(resolved.Id, Is.EqualTo(resolved2.Id), "because they are resolved within the same transaction");

				serviceUsed = resolved;

				tx.Complete();

				Assert.That(resolved.Disposed, "the item should be disposed at the completion of the tx");
			}

			Assert.That(serviceUsed.Disposed, Is.True);
		}

		[Test]
		public void Concurrent_DependentTransaction_AndDisposing()
		{
			// given
			var container = GetContainer();
			var childStarted = new ManualResetEvent(false);
			var childComplete = new ManualResetEvent(false);

			// exports from actions, to assert end-state
			IPerTxService serviceUsed;

			// when
			Exception possibleException = null;
			using (var scope = container.ResolveScope<Service>())
			using (var manager = container.ResolveScope<ITransactionManager>())
			using (var tx = manager.Service.CreateTransaction().Value.Transaction)
			{
				var resolved = scope.Service.DoWork();
				var parentId = resolved.Id;

				// create a child transaction
				var createdTx2 = manager.Service.CreateTransaction(new DefaultTransactionOptions { Fork = true }).Value;

				Assert.That(createdTx2.ShouldFork, Is.True, "because we're in an ambient and have specified the option");
				Assert.That(manager.Service.Count, Is.EqualTo(1), "transaction count correct");

				ThreadPool.QueueUserWorkItem(_ =>
				{
					IPerTxService perTxService;

					try
					{
						using (createdTx2.GetForkScope())
						using (var tx2 = createdTx2.Transaction)
						{
							perTxService = scope.Service.DoWork();
							// this time the ids should be different and we should only have one active transaction in this context
							Assert.That(perTxService.Id, Is.Not.SameAs(parentId));
							Assert.That(manager.Service.Count, Is.EqualTo(1), "transaction count correct");

							// tell parent it can go on and complete
							childStarted.Set();

							Assert.That(perTxService.Disposed, Is.False);

							tx2.Complete();
						}

						// perTxService.Disposed is either true or false at this point depending on the interleaving
						//Assert.That(perTxService.Disposed, Is.???, "becuase dependent transaction hasn't fired its parent TransactionCompleted event, or it HAS done so and it IS disposed");
					}
					catch (Exception ex)
					{
						possibleException = ex;
						logger.Debug("child fault", ex);
					}
					finally
					{
						logger.Debug("child finally");
						childComplete.Set();
					}
				});
				childStarted.WaitOne();

				serviceUsed = resolved;

				Assert.That(resolved.Disposed, Is.False, "the item should be disposed at the completion of the tx");

				tx.Complete();

				Assert.That(resolved.Disposed, "the item should be disposed at the completion of the tx");
			}

			Assert.That(serviceUsed.Disposed, Is.True);

			childComplete.WaitOne();

			// throw any thread exceptions
			if (possibleException != null)
			{
				Console.WriteLine(possibleException);
				Assert.Fail();
			}

			// the component burden in this one should not throw like the log trace below!
			container.Dispose();
			/*Castle.Facilities.AutoTx.Tests.PerTransactionLifestyle_Releasing: 2011-04-26 16:23:01,859 [9] DEBUG - child finally
Castle.Facilities.AutoTx.Lifestyles.PerTransactionLifestyleManagerBase: 2011-04-26 16:23:01,861 [TestRunnerThread] DEBUG - transaction#604654c5-b9bd-44cb-be20-9fc6118308d6:1:1 completed, maybe releasing object '(1, Castle.Facilities.AutoTx.Tests.DisposeMeOnce)'
Castle.Facilities.AutoTx.Lifestyles.PerTransactionLifestyleManagerBase: 2011-04-26 16:23:01,861 [TestRunnerThread] DEBUG - last item of 'Castle.Facilities.AutoTx.Tests.DisposeMeOnce' per-tx; releasing it
Castle.Facilities.AutoTx.Lifestyles.PerTransactionLifestyleManagerBase: 2011-04-26 16:23:01,870 [TestRunnerThread] DEBUG - transaction#604654c5-b9bd-44cb-be20-9fc6118308d6:1:2 completed, maybe releasing object '(1, Castle.Facilities.AutoTx.Tests.DisposeMeOnce)'
Castle.Facilities.AutoTx.Lifestyles.PerTransactionLifestyleManagerBase: 2011-04-26 16:23:01,870 [TestRunnerThread] DEBUG - last item of 'Castle.Facilities.AutoTx.Tests.DisposeMeOnce' per-tx; releasing it
Castle.Facilities.AutoTx.Testing.ResolveScope<ITransactionManager>: 2011-04-26 16:23:01,871 [TestRunnerThread] DEBUG - disposing resolve scope
Castle.Facilities.AutoTx.Testing.ResolveScope<Service>: 2011-04-26 16:23:01,872 [TestRunnerThread] DEBUG - disposing resolve scope
Test 'Castle.Facilities.AutoTx.Tests.PerTransactionLifestyle_Releasing.Concurrent_DependentTransaction_AndDisposing' failed:
	disposed DisposeMeOnce twice
	PerTransactionLifestyle_Releasing.cs(277,0): at Castle.Facilities.AutoTx.Tests.DisposeMeOnce.Dispose()
	at Castle.MicroKernel.LifecycleConcerns.DisposalConcern.Apply(ComponentModel model, Object component)
	at Castle.MicroKernel.LifecycleConcerns.LateBoundConcerns.Apply(ComponentModel model, Object component)
	at Castle.MicroKernel.ComponentActivator.DefaultComponentActivator.ApplyConcerns(IEnumerable`1 steps, Object instance)
	at Castle.MicroKernel.ComponentActivator.DefaultComponentActivator.ApplyDecommissionConcerns(Object instance)
	at Castle.MicroKernel.ComponentActivator.DefaultComponentActivator.InternalDestroy(Object instance)
	at Castle.MicroKernel.ComponentActivator.AbstractComponentActivator.Destroy(Object instance)
	at Castle.MicroKernel.Lifestyle.AbstractLifestyleManager.Release(Object instance)
	Lifestyles\WrapperResolveLifestyleManager.cs(89,0): at Castle.Facilities.AutoTx.Lifestyles.WrapperResolveLifestyleManager`1.Release(Object instance)
	at Castle.MicroKernel.Handlers.DefaultHandler.ReleaseCore(Object instance)
	at Castle.MicroKernel.Handlers.AbstractHandler.Release(Object instance)
	at Castle.MicroKernel.Burden.Release(IReleasePolicy policy)
	at Castle.MicroKernel.Releasers.AllComponentsReleasePolicy.Dispose()
	at Castle.MicroKernel.DefaultKernel.Dispose()
	at Castle.Windsor.WindsorContainer.Dispose()
	PerTransactionLifestyle_Releasing.cs(180,0): at Castle.Facilities.AutoTx.Tests.PerTransactionLifestyle_Releasing.Concurrent_DependentTransaction_AndDisposing()*/
		}

		private WindsorContainer GetContainer()
		{
			var container = new WindsorContainer();
			container.AddFacility<AutoTxFacility>();
			container.AddFacility<FactorySupportFacility>();
			container.AddFacility<TypedFactoryFacility>();
			container.Register(
				Component.For<IPerTxServiceFactory>()
					.Instance(new ServiceFactory())
					.LifeStyle.Singleton
					.Named("per-tx-session.factory"),
				Component.For<IPerTxService>()
					.LifeStyle.PerTransaction()
					.Named("per-tx-session")
					.UsingFactoryMethod(k =>
					{
						var factory = k.Resolve<IPerTxServiceFactory>("per-tx-session.factory");
						var s = factory.CreateService();
						return s;
					}),
				Component.For<Service>(),
				Component.For<ServiceWithDirectDep>());

			return container;
		}
	}

	public class ServiceWithDirectDep
	{

		// ReSharper disable UnusedParameter.Local
		public ServiceWithDirectDep(IPerTxService service)
		{
			Contract.Requires(service != null, "service is null");
		}
		// ReSharper restore UnusedParameter.Local

		[Transaction]
		public virtual void DoWork()
		{
			Assert.Fail("IPerTxService is resolved in the c'tor but is per-tx, so DoWork should never be called as lifestyle throws exception");
		}
	}

	public class Service
	{
		private readonly Func<IPerTxService> factoryMethod;

		public Service(Func<IPerTxService> factoryMethod)
		{
			Contract.Requires(factoryMethod != null);

			this.factoryMethod = factoryMethod;
		}

		// return the used service so we can assert on it
		public virtual IPerTxService DoWork()
		{
			var perTxService = factoryMethod();
			Console.WriteLine(perTxService.SayHello());
			return perTxService;
		}
	}

	public interface IPerTxServiceFactory
	{
		IPerTxService CreateService();
	}

	public class ServiceFactory : IPerTxServiceFactory
	{
		public IPerTxService CreateService()
		{
			// each instance should have a new guid
			return new DisposeMeOnce(Guid.NewGuid());
		}
	}

	public interface IPerTxService
	{
		string SayHello();

		bool Disposed { get; }
		Guid Id { get; }
	}

	public class DisposeMeOnce : IPerTxService, IDisposable
	{
		private readonly Guid newGuid;
		private bool disposed;

		public DisposeMeOnce(Guid newGuid)
		{
			this.newGuid = newGuid;
		}

		public bool Disposed
		{
			get { return disposed; }
		}

		public Guid Id
		{
			get { return newGuid; }
		}

		public string SayHello()
		{
			return "Hello from disposable service!";
		}

		public void Dispose()
		{
			if (disposed)
				Assert.Fail("disposed DisposeMeOnce twice");

			disposed = true;
		}
	}
	// ReSharper restore InconsistentNaming
}