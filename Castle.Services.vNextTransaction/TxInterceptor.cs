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
using System.Transactions;
using Castle.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using log4net;

namespace Castle.Services.vNextTransaction
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
			Contract.Ensures(_State == InterceptorState.Active);
			Contract.Assume(_State == InterceptorState.Active || _State == InterceptorState.Initialized);
			Contract.Assume(invocation != null);

			var txMethod = _MetaInfo.Do(x => x.AsTransactional(invocation.Method.DeclaringType.IsInterface
			                                                	? invocation.MethodInvocationTarget
			                                                	: invocation.Method));

			var tx = txMethod.Do(x => _Kernel.Resolve<ITxManager>().CreateTransaction(x));
			
			_State = InterceptorState.Active;

			if (!tx.HasValue)
			{
				if (txMethod.HasValue && txMethod.Value.TransactionMode == TransactionScopeOption.Suppress)
					using (new TxScope(null))
						invocation.Proceed();

				else invocation.Proceed();
				return;
			}

			_Logger.DebugFormat("proceeding with transaction");
			Contract.Assume(!tx.HasValue || tx.Value.State == TransactionState.Active, "from post-condition of ITxManager CreateTransaction in the (HasValue -> ...)-case");

			using (new TxScope(tx.HasValue ? tx.Value.Inner : null))
			{
				try
				{
					invocation.Proceed();
					if (tx.HasValue) tx.Value.Complete();
				}
				catch (TransactionException ex)
				{
					_Logger.Fatal("internal error in transaction system", ex);
					throw;
				}
				catch (Exception)
				{
					if (tx.HasValue) tx.Value.Rollback();
					throw;
				}
				finally
				{
					if (tx.HasValue) tx.Value.Dispose();
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
}