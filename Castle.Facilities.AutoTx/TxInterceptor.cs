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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Transactions;
using Castle.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.Services.Transaction;
using log4net;
using TransactionException = Castle.Services.Transaction.Exceptions.TransactionException;

namespace Castle.Facilities.AutoTx
{
	internal class TxInterceptor : IInterceptor, IOnBehalfAware
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TxInterceptor));

		private enum InterceptorState
		{
			Constructed,
			Initialized,
			Active
		}

		private InterceptorState _State;
		private readonly IKernel _Kernel;
		private readonly ITxMetaInfoStore _Store;
		private Maybe<TxClassMetaInfo> _MetaInfo;

		public TxInterceptor(IKernel kernel, ITxMetaInfoStore store)
		{
			Contract.Requires(kernel != null, "kernel must be non null");
			Contract.Requires(store != null, "store must be non null");
			Contract.Ensures(_State == InterceptorState.Constructed);

			_Logger.DebugFormat("created transaction interceptor");

			_Kernel = kernel;
			_Store = store;
			_State = InterceptorState.Constructed;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_State != InterceptorState.Initialized || _MetaInfo != null);
		}

		void IInterceptor.Intercept(IInvocation invocation)
		{
			Contract.Assume(_State == InterceptorState.Active || _State == InterceptorState.Initialized);
			Contract.Assume(invocation != null);

			var txManager = _Kernel.Resolve<ITxManager>();

			var mTxMethod = _MetaInfo.Do(x => x.AsTransactional(invocation.Method.DeclaringType.IsInterface
			                                                	? invocation.MethodInvocationTarget
			                                                	: invocation.Method));

			var mTxData = mTxMethod.Do(x => txManager.CreateTransaction(x));

			_State = InterceptorState.Active;
			
			if (!mTxData.HasValue)
			{
				if (mTxMethod.HasValue && mTxMethod.Value.Mode == TransactionScopeOption.Suppress)
					using (new TxScope(null))
						invocation.Proceed();

				else invocation.Proceed();

				return;
			}

			var transaction = mTxData.Value.Transaction;

			Contract.Assume(transaction.State == TransactionState.Active, 
				"from post-condition of ITxManager CreateTransaction in the (HasValue -> ...)-case");

			// TODO 3.0GA: implement functionality for getting tasks and awating them
#pragma warning disable 168
			Task forkCase;
#pragma warning restore 168
			if (mTxData.Value.ShouldFork)
				// TODO: Handle case where child transaction aborts and task is never waited upon!
				ForkCase(invocation, mTxData.Value);
			else
				SynchronizedCase(invocation, transaction);
		}

		// TODO: implement WaitAll-semantics with returned task
		private static Task ForkCase(IInvocation invocation, ICreatedTransaction txData)
		{
			Contract.Requires(txData.Transaction.State == TransactionState.Active);
			Contract.Assume(txData.Transaction.Inner is DependentTransaction);
			
			_Logger.DebugFormat("fork case");

			return Task.Factory.StartNew(t =>
			{
				bool hasException = false;
				var tuple = (Tuple<IInvocation, ICreatedTransaction, string>)t;
				var dependent = tuple.Item2.Transaction.Inner as DependentTransaction;
				using (tuple.Item2.GetForkScope())
				{
					try
					{
						if (_Logger.IsDebugEnabled)
							_Logger.DebugFormat("calling proceed on tx#{0}", tuple.Item3);

						using (var ts = new TransactionScope(dependent))
						{
							tuple.Item1.Proceed();

							if (_Logger.IsDebugEnabled)
								_Logger.DebugFormat("calling complete on TransactionScope for tx#{0}", tuple.Item3);

							ts.Complete();
						}
					}
					catch (TransactionAbortedException ex)
					{
						// if we have aborted the transaction, we both warn and re-throw the exception
						hasException = true;
						_Logger.Warn("transaction aborted", ex);
						throw new TransactionAbortedException("Parallel/forked transaction aborted! See inner exception for details.", ex);
					}
					catch (Exception)
					{
						hasException = true;
						throw;
					}
					finally
					{
						if (_Logger.IsDebugEnabled)
							_Logger.Debug("in finally-clause");

						if (!hasException)
							dependent.Complete();

						// See footnote at end of file
					}
				}
			}, Tuple.Create(invocation, txData, txData.Transaction.LocalIdentifier));
		}

		private static void SynchronizedCase(IInvocation invocation, ITransaction transaction)
		{
			Contract.Requires(transaction.State == TransactionState.Active);
			_Logger.DebugFormat("synchronized case");

			using (new TxScope(transaction.Inner))
			{
				try
				{
					invocation.Proceed();

					if (transaction.State == TransactionState.Active)
						transaction.Complete();

					else _Logger.WarnFormat("transaction was in state {0}, so it cannot be completed. the 'consumer' method, so to speak, might have rolled it back.", transaction.State);
				}
				catch (TransactionAbortedException)
				{
					// if we have aborted the transaction, we both warn and re-throw the exception
					_Logger.Warn("transaction aborted - synchronized case");
					throw;
				}
				catch (TransactionException ex)
				{
					if (_Logger.IsFatalEnabled)
						_Logger.Fatal("internal error in transaction system - synchronized case", ex);

					throw;
				}
				catch (Exception)
				{
					if (_Logger.IsErrorEnabled)
						_Logger.ErrorFormat("caught exception, rolling back transaction - synchronized case - tx#{0}", transaction.LocalIdentifier);

					transaction.Rollback();
					throw;
				}
				finally
				{
					if (_Logger.IsDebugEnabled)
						_Logger.DebugFormat("dispoing transaction - synchronized case - tx#{0}", transaction.LocalIdentifier);

					transaction.Dispose();
				}
			}
		}

		void IOnBehalfAware.SetInterceptedComponentModel(ComponentModel target)
		{
			Contract.Ensures(_MetaInfo != null);
			Contract.Assume(target != null && target.Implementation != null);
			_MetaInfo = _Store.GetMetaFromType(target.Implementation);
			_State = InterceptorState.Initialized;
		}
	}

	// * In their code: http://msdn.microsoft.com/en-us/library/ms229976%28v=vs.90%29.aspx#Y17
	//
	// MSDN article is wrong; transaction is committed twice now:
	//                      System.InvalidOperationException was unhandled by user code
	//Message=The operation is not valid for the current state of the enlistment.
	//Source=System.Transactions
	//StackTrace:
	//     at System.Transactions.EnlistmentState.ForceRollback(InternalEnlistment enlistment, Exception e)
	//     at System.Transactions.PreparingEnlistment.ForceRollback(Exception e)
	//     at NHibernate.Transaction.AdoNetWithDistributedTransactionFactory.DistributedTransactionContext.System.Transactions.IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment) in d:\CSharp\NH\NH\nhibernate\src\NHibernate\Transaction\AdoNetWithDistributedTransactionFactory.cs:line 129
	//     at System.Transactions.VolatileEnlistmentPreparing.EnterState(InternalEnlistment enlistment)
	//     at System.Transactions.VolatileEnlistmentActive.ChangeStatePreparing(InternalEnlistment enlistment)
	//     at System.Transactions.TransactionStatePhase0.Phase0VolatilePrepareDone(InternalTransaction tx)
	//     at System.Transactions.EnlistableStates.CompleteBlockingClone(InternalTransaction tx)
	//     at System.Transactions.DependentTransaction.Complete()
	//     at Castle.Facilities.AutoTx.TxInterceptor.<ForkCase>b__4(Object t) in f:\code\castle\Castle.Services.Transaction\src\Castle.Facilities.AutoTx\TxInterceptor.cs:line 144
	//     at System.Threading.Tasks.Task.InnerInvoke()
	//     at System.Threading.Tasks.Task.Execute()
	//InnerException: 
	//dependent.Complete();

	// Second wrong; calling dispose on it
	// MSDN-article is wrong; transaction is disposed twice in their code:
	// tuple.Item2.Dispose(); 
}