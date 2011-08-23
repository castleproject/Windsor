using System;
using System.Threading;
using Castle.Facilities.AutoTx.Testing;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Transactions;
using Castle.Windsor;
using NUnit.Framework;
using System.Linq;

namespace Castle.Facilities.AutoTx.Tests
{
	public class MultipleThreads_TransactionBookKeeping
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<MyService>());
			ThreadPool.SetMinThreads(5, 5);
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void Count_InTheAmbientContext_IsOne()
		{
			var parentHasAsserted = new ManualResetEvent(false);
			var childHasStarted = new ManualResetEvent(false);

			using (var manager = _Container.ResolveScope<ITransactionManager>())
			using (var scope = _Container.ResolveScope<MyService>())
			{
				Assert.That(manager.Service.Count, Is.EqualTo(0));
				scope.Service.VerifyInAmbient(() =>
				{
					Assert.That(manager.Service.Count, Is.EqualTo(1));

					scope.Service.VerifyBookKeepingInFork(() =>
					{
						childHasStarted.Set();
						parentHasAsserted.WaitOne();

						Assert.That(manager.Service.Count, Is.EqualTo(1), "because we're in a different call-context");
					}, null, null);

					childHasStarted.WaitOne();

					try
					{
						Assert.That(manager.Service.Count, Is.EqualTo(1), "because we're in the same call context, and the dependent transaction is not our 'current', so the count needs to match");
					}
					finally
					{
						parentHasAsserted.Set();
					}
				});
			}
		}

		[Test]
		public void InterleavingWhereChildCommit_IsOutstanding_OverParent()
		{
			// ReSharper disable ConvertToLambdaExpression)
			using (var manager = _Container.ResolveScope<TransactionManager>())
			using (var scope = _Container.ResolveScope<MyService>())
			{
				var parentCompleted = new ManualResetEvent(false);
				var childHasCompleted = new ManualResetEvent(false);
				TransactionInterceptor.Finally = childHasCompleted;
				const string exMsg = "something went wrong, but parent has completed";

				try {
					scope.Service.VerifyInAmbient(() => {
						// fixing interleaving
						var top = (Transaction)((ITransactionManager)manager.Service).CurrentTopTransaction.Value;
						top.BeforeTopComplete = () => childHasCompleted.WaitOne();

						scope.Service.VerifyBookKeepingInFork(() =>
						{
							throw new ApplicationException(exMsg);
						});

						childHasCompleted.WaitOne();
					});
					Assert.Fail("No exception was thrown!");
				}
				catch (AggregateException ex)
				{
					Assert.That(ex.InnerExceptions.Any(x => x is ApplicationException && x.Message == exMsg));
				}
			}
			GC.WaitForPendingFinalizers(); // because tasks throw on finalize
			// ReSharper restore ConvertToLambdaExpression
		}

			/* ------ Test started: Assembly: Castle.Facilities.AutoTx.Tests.dll ------

Castle.Facilities.AutoTx.AutoTxFacility: 2011-04-20 10:13:45,513 [TestRunnerThread] DEBUG - initializing AutoTxFacility
Castle.Facilities.AutoTx.Lifestyles.WrapperResolveLifestyleManager<PerTransactionLifestyleManager>: 2011-04-20 10:13:45,706 [TestRunnerThread] DEBUG - initializing (for component: Castle.Services.Transaction.IDirectoryAdapter)
Castle.Facilities.AutoTx.Lifestyles.PerTransactionLifestyleManagerBase: 2011-04-20 10:13:45,742 [TestRunnerThread] DEBUG - created
Castle.Facilities.AutoTx.Lifestyles.WrapperResolveLifestyleManager<PerTransactionLifestyleManager>: 2011-04-20 10:13:45,743 [TestRunnerThread] DEBUG - initialized
Castle.Facilities.AutoTx.Lifestyles.WrapperResolveLifestyleManager<PerTransactionLifestyleManager>: 2011-04-20 10:13:45,744 [TestRunnerThread] DEBUG - initializing (for component: Castle.Services.Transaction.IFileAdapter)
Castle.Facilities.AutoTx.Lifestyles.PerTransactionLifestyleManagerBase: 2011-04-20 10:13:45,744 [TestRunnerThread] DEBUG - created
Castle.Facilities.AutoTx.Lifestyles.WrapperResolveLifestyleManager<PerTransactionLifestyleManager>: 2011-04-20 10:13:45,744 [TestRunnerThread] DEBUG - initialized
Castle.Facilities.AutoTx.AutoTxFacility: 2011-04-20 10:13:45,760 [TestRunnerThread] DEBUG - initialized AutoTxFacility
Castle.Facilities.AutoTx.Testing.RsolveScope<ITransactionManager>: 2011-04-20 10:13:45,803 [TestRunnerThread] DEBUG - creating
Castle.Facilities.AutoTx.Testing.RsolveScope<IMyService>: 2011-04-20 10:13:45,804 [TestRunnerThread] DEBUG - creating
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,808 [TestRunnerThread] DEBUG - created transaction interceptor
Castle.Services.Transaction.Activity: 2011-04-20 10:13:45,954 [TestRunnerThread] DEBUG - pushing tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:1
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,955 [TestRunnerThread] DEBUG - synchronized case
Castle.Services.Transaction.Activity: 2011-04-20 10:13:45,976 [TestRunnerThread] DEBUG - pushing tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:2
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,977 [TestRunnerThread] DEBUG - fork case
Castle.Services.Transaction.Activity: 2011-04-20 10:13:45,978 [10] DEBUG - pushing tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:2
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,979 [10] DEBUG - calling proceed on tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:2
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,985 [10] DEBUG - in finally-clause
Castle.Services.Transaction.Activity: 2011-04-20 10:13:45,986 [10] DEBUG - popping tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:2
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,987 [TestRunnerThread] WARN  - transaction aborted - synchronized case
Castle.Facilities.AutoTx.TransactionInterceptor: 2011-04-20 10:13:45,988 [TestRunnerThread] DEBUG - dispoing transaction - synchronized case - tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:1
Castle.Services.Transaction.Activity: 2011-04-20 10:13:45,988 [TestRunnerThread] DEBUG - popping tx#9b8a7f45-59e7-4904-81b9-8ad5d76fad27:1:2
Castle.Facilities.AutoTx.Testing.RsolveScope<IMyService>: 2011-04-20 10:13:45,989 [TestRunnerThread] DEBUG - disposing resolve scope
Castle.Facilities.AutoTx.Testing.RsolveScope<ITransactionManager>: 2011-04-20 10:13:45,990 [TestRunnerThread] DEBUG - disposing resolve scope
Test 'Castle.Facilities.AutoTx.Tests.MultipleThreads_TransactionBookKeeping.InterleavingWhereChildCommit_IsOutstanding_OverParent' failed:
	System.Transactions.TransactionAbortedException : The transaction has aborted.
	at System.Transactions.TransactionStateAborted.EndCommit(InternalTransaction tx)
	at System.Transactions.CommittableTransaction.Commit()
	Transaction.cs(208,0): at Castle.Services.Transaction.Transaction.Castle.Services.Transaction.ITransaction.Complete()
	TransactionInterceptor.cs(167,0): at Castle.Facilities.AutoTx.TransactionInterceptor.SynchronizedCase(IInvocation invocation, ITransaction transaction)
	TransactionInterceptor.cs(103,0): at Castle.Facilities.AutoTx.TransactionInterceptor.Castle.DynamicProxy.IInterceptor.Intercept(IInvocation invocation)
	at Castle.DynamicProxy.AbstractInvocation.Proceed()
	at Castle.Proxies.IMyServiceProxy.VerifyInAmbient(Action a)
	MultipleThreads_TransactionBookKeeping.cs(65,0): at Castle.Facilities.AutoTx.Tests.MultipleThreads_TransactionBookKeeping.InterleavingWhereChildCommit_IsOutstanding_OverParent()

---- UNHANDLED EXCEPTION ----
Thread Name: <No Name>
System.AggregateException: A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread. ---> System.ApplicationException: something went wrong, but parent has completed
   at Castle.Facilities.AutoTx.Tests.MultipleThreads_TransactionBookKeeping.<>c__DisplayClassd.<InterleavingWhereChildCommit_IsOutstanding_OverParent>b__a() in F:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx.Tests\MultipleThreads_TransactionBookKeeping.cs:line 70
   at Castle.Facilities.AutoTx.Tests.TestClasses.MyService.VerifyBookKeepingInFork(Action a) in F:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx.Tests\TestClasses\MyService.cs:line 67
   at Castle.Proxies.Invocations.IMyService_VerifyBookKeepingInFork.InvokeMethodOnTarget()
   at Castle.DynamicProxy.AbstractInvocation.Proceed() in e:\OSS.Code\Castle.Core\src\Castle.Core\DynamicProxy\AbstractInvocation.cs:line 144
   at Castle.Facilities.AutoTx.TransactionInterceptor.<ForkCase>b__4(Object t) in F:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx\TransactionInterceptor.cs:line 126
   at System.Threading.Tasks.Task.InnerInvoke()
   at System.Threading.Tasks.Task.Execute()
   --- End of inner exception stack trace ---
   at System.Threading.Tasks.TaskExceptionHolder.Finalize()
---> (Inner Exception #0) System.ApplicationException: something went wrong, but parent has completed
   at Castle.Facilities.AutoTx.Tests.MultipleThreads_TransactionBookKeeping.<>c__DisplayClassd.<InterleavingWhereChildCommit_IsOutstanding_OverParent>b__a() in F:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx.Tests\MultipleThreads_TransactionBookKeeping.cs:line 70
   at Castle.Facilities.AutoTx.Tests.TestClasses.MyService.VerifyBookKeepingInFork(Action a) in F:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx.Tests\TestClasses\MyService.cs:line 67
   at Castle.Proxies.Invocations.IMyService_VerifyBookKeepingInFork.InvokeMethodOnTarget()
   at Castle.DynamicProxy.AbstractInvocation.Proceed() in e:\OSS.Code\Castle.Core\src\Castle.Core\DynamicProxy\AbstractInvocation.cs:line 144
   at Castle.Facilities.AutoTx.TransactionInterceptor.<ForkCase>b__4(Object t) in F:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx\TransactionInterceptor.cs:line 126
   at System.Threading.Tasks.Task.InnerInvoke()
   at System.Threading.Tasks.Task.Execute()<---


0 passed, 1 failed, 0 skipped, took 2,50 seconds (NUnit 2.5.5).*/
	}
}