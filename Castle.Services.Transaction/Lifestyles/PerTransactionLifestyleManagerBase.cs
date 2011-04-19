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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.Services.Transaction.Monads;
using log4net;
using System.Linq;

namespace Castle.Services.Transaction.Lifestyles
{
	/// <summary>
	/// This lifestyle manager is responsible for disposing components
	/// at the same time as the transaction is completed, i.e. the transction
	/// either Aborts, becomes InDoubt or Commits.
	/// </summary>
	[Serializable]
	public abstract class PerTransactionLifestyleManagerBase : AbstractLifestyleManager
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (PerTransactionLifestyleManagerBase));

		private readonly Dictionary<string, Tuple<uint, object>> _Storage = new Dictionary<string, Tuple<uint, object>>();

		protected readonly ITxManager _Manager;
		protected bool _Disposed;

		public PerTransactionLifestyleManagerBase(ITxManager manager)
		{
			Contract.Requires(manager != null);
			Contract.Ensures(_Manager != null);
			_Logger.DebugFormat("created");
			_Manager = manager;
		}

		// this method is not thread-safe
		public override void Dispose()
		{
			Contract.Ensures(_Disposed);

			if (_Disposed)
			{
				_Logger.Info("repeated call to Dispose. will show stack-trace if log4net is in debug mode as the next log line. this method call is idempotent");

				if (_Logger.IsDebugEnabled)
					_Logger.Debug(new StackTrace().ToString());

				return;
			}

			try
			{
				lock (ComponentActivator)
				{
					if (_Storage.Count > 0)
					{
						var items = string.Join(
							string.Format(", {0}", Environment.NewLine),
							_Storage
								.Select(x => string.Format("(id: {0}, item: {1})", x.Key, x.Value.ToString()))
								.ToArray());

						_Logger.WarnFormat("Storage contains {0} items! Items: {{ {1} }}",
						                   _Storage.Count,
						                   items);
					}

					// release all items
					foreach (var tuple in _Storage)
						base.Release(tuple.Value.Item2);

					_Storage.Clear();
				}

			}
			finally
			{
				_Disposed = true;
			}
		}

		public override object Resolve(CreationContext context)
		{
			Contract.Ensures(Contract.Result<object>() != null);

			if (_Logger.IsDebugEnabled)
				_Logger.DebugFormat("resolving service '{0}' using PerTransaction lifestyle", context.Handler.Service);

			if (_Disposed)
				throw new ObjectDisposedException("PerTransactionLifestyleManagerBase", "You cannot resolve with a disposed lifestyle.");

			if (!GetSemanticTransactionForLifetime().HasValue)
				throw new MissingTransactionException();

			var transaction = GetSemanticTransactionForLifetime().Value;

			Tuple<uint, object> instance;
			// unique key per service and per top transaction identifier
			var localIdentifier = transaction.LocalIdentifier;
			var key = localIdentifier + "|" + context.Handler.Service.GetHashCode();

			if (!_Storage.TryGetValue(key, out instance))
			{
				lock (ComponentActivator)
				{
					if (_Logger.IsDebugEnabled)
						_Logger.DebugFormat("component for key '{0}' not found in per-tx storage. creating new instance.", key);

					if (!_Storage.TryGetValue(key, out instance))
					{
						instance = _Storage[key] = Tuple.Create(1u, base.Resolve(context));

						transaction.Inner.TransactionCompleted += (sender, args) =>
						{
							var id = localIdentifier;
							if (_Logger.IsDebugEnabled)
								_Logger.DebugFormat("transaction#{0} completed, disposing object '{1}'", id, instance);

							lock (ComponentActivator)
							{
								Tuple<uint, object> counter = _Storage[key];

								if (counter.Item1 == 1)
								{
									if (_Logger.IsDebugEnabled)
										_Logger.DebugFormat("last item of '{0}' per-tx; releasing it", counter.Item2);

									// this might happen if the transaction outlives the service; the transaction might also notify transaction fron a timer, i.e.
									// not synchronously.
									if (!_Disposed)
									{
										Contract.Assume(_Storage.Count > 0);

										_Storage.Remove(key);
										Release(counter.Item2);
									}
								}
								else
								{
									if (_Logger.IsDebugEnabled)
										_Logger.DebugFormat("{0} item(s) of '{1}' left in per-tx storage", counter.Item1 - 1, counter.Item2);

									_Storage[key] = Tuple.Create(counter.Item1 - 1, counter.Item2);
								}
							}
						};
					}
				}
			}
				
			Contract.Assume(instance.Item2 != null, "resolve throws otherwise");

			return instance.Item2;
		}

		/// <summary>
		/// Gets the 'current' transaction; a semantic defined by the inheritors of this class.
		/// </summary>
		/// <returns>Maybe a current transaction as can be found in the transaction manager.</returns>
		protected internal abstract Maybe<ITransaction> GetSemanticTransactionForLifetime();
	}

	/// <summary>
	/// A lifestyle manager that resolves a fresh instance for every transaction. In my opinion, this 
	/// is the most semantically correct option of the two per-transaction lifestyle managers: it's possible
	/// to audit your code to verify that sub-sequent calls to services don't start new transactions on their own.
	/// With this lifestyle, code executing in other threads work as expected, as no instances are shared accross these
	/// threads (this refers to the Fork=true option on the TransactionAttribute).
	/// </summary>
	public class PerTransactionLifestyleManager : PerTransactionLifestyleManagerBase
	{
		public PerTransactionLifestyleManager(ITxManager manager) 
			: base(manager)
		{
			Contract.Requires(manager != null);
		}

		#region Overrides of PerTransactionLifestyleManagerBase

		protected internal override Maybe<ITransaction> GetSemanticTransactionForLifetime()
		{
			if (_Disposed)
				throw new ObjectDisposedException("PerTransactionLifestyleManager", "The lifestyle manager is disposed and cannot be used.");

			return _Manager.CurrentTransaction;
		}

		#endregion
	}

	/// <summary>
	/// A lifestyle manager for every top transaction in the current call context. This lifestyle is great
	/// for components that are thread-safe and need to monitor/handle items in both the current thread
	/// and any forked method invocations. It's also favoring memory if your application is single threaded,
	/// as there's no need to create a new component every sub-transaction. (this refers to the Fork=true option
	/// on the TransactionAttribute).
	/// </summary>
	public class PerTopTransactionLifestyleManager : PerTransactionLifestyleManagerBase
	{
		public PerTopTransactionLifestyleManager(ITxManager manager)
			: base(manager)
		{
			Contract.Requires(manager != null);
		}

		#region Overrides of PerTransactionLifestyleManagerBase

		protected internal override Maybe<ITransaction> GetSemanticTransactionForLifetime()
		{
			if (_Disposed)
				throw new ObjectDisposedException("PerTopTransactionLifestyleManager", "The lifestyle manager is disposed and cannot be used.");

			return _Manager.CurrentTopTransaction;
		}

		#endregion
	}
}