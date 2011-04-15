using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.Services.vNextTransaction;
using Castle.Services.vNextTransaction.Utils;
using log4net;
using NHibernate;
using System.Linq;

namespace Castle.Facilities.NHibernate
{
	[Serializable]
	public class PerTransactionLifestyleManager : AbstractLifestyleManager
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (PerTransactionLifestyleManager));

		private readonly Dictionary<string, Tuple<uint, object>> _Storage = new Dictionary<string, Tuple<uint, object>>();

		private readonly ITxManager _Manager;

		private bool _Disposed;

		public PerTransactionLifestyleManager(ITxManager manager)
		{
			Contract.Requires(manager != null);
			_Logger.DebugFormat("created");
			_Manager = manager;
		}

		// this method is not thread-safe
		public override void Dispose()
		{
			if (_Disposed) return;

			try
			{
				Contract.Assume(_Storage.Count == 0);

				if (_Storage.Count > 0)
					_Logger.WarnFormat("Storage contains {0} items! Items: {{ {1} }}",
					                   _Storage.Count,
					                   string.Join(", ", _Storage.Values.Select(x => x.ToString()).ToArray()));
			}
			finally
			{
				_Disposed = true;
			}
		}

		public override object Resolve(CreationContext context)
		{
			Contract.Ensures(Contract.Result<object>() != null);

			_Logger.DebugFormat("resolving context '{0}' using PerTransaction lifestyle", context);

			if (!_Manager.CurrentTopTransaction.HasValue)
				throw new MissingTransactionException();

			var transaction = _Manager.CurrentTopTransaction.Value.Inner;

			Tuple<uint, object> instance;
			// unique key per service and per top transaction identifier
			var key = transaction.TransactionInformation.LocalIdentifier + "|" + context.Handler.Service.GetHashCode();

			if (!_Storage.TryGetValue(key, out instance))
			{
				lock (ComponentActivator)
				{
					if (!_Storage.TryGetValue(key, out instance))
						instance = _Storage[key] = Tuple.Create(0u, base.Resolve(context));
				}
			}
				
			Contract.Assume(instance.Item2 != null, "resolve throws otherwise");

			transaction.TransactionCompleted += (sender, args) =>
			{
				if (_Logger.IsDebugEnabled)
					_Logger.DebugFormat("transaction#{0} completed, disposing object '{1}'",
						args.Transaction.TransactionInformation.LocalIdentifier,
						instance);

				lock (ComponentActivator)
				{
					Tuple<uint, object> counter = _Storage[key];

					if (counter.Item1 == 1)
					{
						_Storage.Remove(key);
						Release(counter.Item2);
					}
					else
					{
						_Storage[key] = Tuple.Create(counter.Item1 - 1, counter.Item2);
					}
				}
			};

			return instance.Item2;
		}
	}
}