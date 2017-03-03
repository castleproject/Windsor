// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.Synchronize
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Threading;
	using System.Windows.Threading;

	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	using System.Security;

	/// <summary>
	///   Intercepts calls to synchronized components and ensures
	///   that they execute in the proper synchronization context.
	/// </summary>
	[Transient]
	internal class SynchronizeInterceptor : IInterceptor, IOnBehalfAware
	{
		private readonly IKernel kernel;
		private SynchronizeMetaInfo metaInfo;
		private readonly SynchronizeMetaInfoStore metaStore;
		private readonly InvocationDelegate safeInvoke = InvokeSafely;

		[ThreadStatic]
		private static SynchronizationContext activeSyncContext;

		private delegate void InvocationDelegate(IInvocation invocation, Result result);

		/// <summary>
		///   Initializes a new instance of the <see cref = "SynchronizeInterceptor" /> class.
		/// </summary>
		/// <param name = "kernel">The kernel.</param>
		/// <param name = "metaStore">The meta store.</param>
		public SynchronizeInterceptor(IKernel kernel, SynchronizeMetaInfoStore metaStore)
		{
			this.kernel = kernel;
			this.metaStore = metaStore;
		}

		/// <summary>
		///   Sets the intercepted ComponentModel.
		/// </summary>
		/// <param name = "target">The targets ComponentModel.</param>
		public void SetInterceptedComponentModel(ComponentModel target)
		{
			metaInfo = metaStore.GetMetaFor(target.Implementation);

		}

		/// <summary>
		/// Intercepts the invocation and applies any necessary
		/// synchronization.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		[SecuritySafeCritical]
		public void Intercept(IInvocation invocation)
		{
			if (InvokeInSynchronizationContext(invocation))
			{
				return;
			}

			if (InvokeWithSynchronizedTarget<ISynchronizeInvoke>(
				invocation, target => target.InvokeRequired == false,
				(target, call, result) => target.Invoke(safeInvoke, new object[] { call, result })))
			{
				return;
			}

			if (InvokeWithSynchronizedTarget<DispatcherObject>(
				invocation, target => target.Dispatcher.CheckAccess(),
				(target, call, result) => target.Dispatcher.Invoke(safeInvoke, DispatcherPriority.Normal, call, result)))
			{
				return;
			}
		}

		/// <summary>
		/// Continues the invocation in a synchronization context
		/// if necessary.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		/// <returns>
		/// 	<c>true</c> if continued; otherwise, <c>false</c>.
		/// </returns>
		[SecurityCritical]
		private bool InvokeInSynchronizationContext(IInvocation invocation)
		{
			if (metaInfo == null)
			{
				return false;
			}

			SynchronizationContext syncContext = null;
			SynchronizationContext prevSyncContext = null;
			var methodInfo = invocation.MethodInvocationTarget;
			var syncContextRef = metaInfo.GetSynchronizedContextFor(methodInfo);

			if (syncContextRef != null)
			{
				syncContext = syncContextRef.Resolve(kernel, CreationContext.CreateEmpty());
				prevSyncContext = SynchronizationContext.Current;
			}
			else
			{
				syncContext = SynchronizationContext.Current;
			}

			if (syncContext != activeSyncContext)
			{
				try
				{
					var result = CreateResult(invocation);

					if (prevSyncContext != null)
					{
						SynchronizationContext.SetSynchronizationContext(syncContext);
					}

					if (syncContext.GetType() == typeof(SynchronizationContext))
					{
						InvokeSynchronously(invocation, result);
					}
					else
					{
						syncContext.Send(state =>
						                 {
							                 activeSyncContext = syncContext;
							                 try
							                 {
								                 InvokeSafely(invocation, result);
							                 }
							                 finally
							                 {
								                 activeSyncContext = null;
							                 }
						                 }, null);
					}
				}
				finally
				{
					if (prevSyncContext != null)
					{
						SynchronizationContext.SetSynchronizationContext(prevSyncContext);
					}
				}
			}
			else
			{
				InvokeSynchronously(invocation, null);
			}

			return true;
		}

		[SecurityCritical]
		private static bool InvokeWithSynchronizedTarget<T>(IInvocation invocation, Func<T, bool> canCallOnThread, Action<T, IInvocation, Result> post) where T : class
		{
			var syncTarget = invocation.InvocationTarget as T;

			if (syncTarget == null)
			{
				return false;
			}

			var result = CreateResult(invocation);

			if (canCallOnThread(syncTarget) == false)
			{
				post(syncTarget, invocation, result);
			}
			else
			{
				InvokeSynchronously(invocation, result);
			}

			return true;
		}

		/// <summary>
		/// Continues the invocation synchronously.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		/// <param name="result">The result holder.</param>
		[SecurityCritical]
		private static void InvokeSynchronously(IInvocation invocation, Result result)
		{
			invocation.Proceed();

			result = result ?? CreateResult(invocation);

			if (result != null)
			{
				CompleteResult(result, true, invocation);
			}
		}

		/// <summary>
		///   Used by the safe synchronization delegate.
		/// </summary>
		/// <param name = "invocation">The invocation.</param>
		/// <param name = "result">The result holder.</param>
		private static void InvokeSafely(IInvocation invocation, Result result)
		{
			if (result == null)
			{
				invocation.Proceed();
			}
			else
			{
				try
				{
					invocation.Proceed();
					CompleteResult(result, false, invocation);
				}
				catch (Exception exception)
				{
					result.SetException(false, exception);
				}
			}
		}

		/// <summary>
		/// Creates the result of the invocation.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		/// <returns>Holds the invocation result.</returns>
		[SecurityCritical]
		private static Result CreateResult(IInvocation invocation)
		{
			Result result = null;
			var returnType = invocation.Method.ReturnType;

			if (returnType != typeof(void))
			{
				if (invocation.ReturnValue == null)
				{
					invocation.ReturnValue = GetDefault(returnType);
				}
				result = new Result();
			}

			Result.Last = result;
			return result;
		}

		/// <summary>
		///   Completes the result of the invocation.
		/// </summary>
		/// <param name = "result">The result.</param>
		/// <param name = "synchronously">true if completeed synchronously.</param>
		/// <param name = "invocation">The invocation.</param>
		private static void CompleteResult(Result result, bool synchronously, IInvocation invocation)
		{
			var parameters = invocation.Method.GetParameters();
			var outs = invocation.Arguments.Where((a, i) => parameters[i].IsOut || parameters[i].ParameterType.IsByRef);
			result.SetValues(synchronously, invocation.ReturnValue, outs.ToArray());
		}

		/// <summary>
		/// Gets the default value for a type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The default value for the type.</returns>
		[SecurityCritical]
		private static object GetDefault(Type type)
		{
			return type.IsValueType ? FormatterServices.GetUninitializedObject(type) : null;
		}
	}
}