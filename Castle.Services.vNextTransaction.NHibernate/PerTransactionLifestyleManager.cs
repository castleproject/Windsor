using System;
using System.Diagnostics.Contracts;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.Services.vNextTransaction;
using log4net;

namespace Castle.Facilities.NHibernate
{
	[Serializable]
	public class PerTransactionLifestyleManager : AbstractLifestyleManager
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (PerTransactionLifestyleManager));

		private readonly ITxManager _Manager;

		public PerTransactionLifestyleManager(ITxManager manager)
		{
			Contract.Requires(manager != null);

			_Manager = manager;
		}

		// Methods
		public override void Dispose()
		{
		}

		public override bool Release(object instance)
		{
			return base.Release(instance);
		}

		public override object Resolve(CreationContext context)
		{
			Contract.Ensures(Contract.Result<object>() != null);

			_Logger.DebugFormat("resolving context '{0}' using PerTransaction lifestyle", context);

			if (!_Manager.CurrentTopTransaction.HasValue)
				throw new MissingTransactionException();

			var instance = base.Resolve(context);

			Contract.Assume(instance != null, "resolve throws otherwise");

			_Manager.CurrentTopTransaction.Value.Inner.TransactionCompleted += (sender, args) =>
			{
				if (_Logger.IsDebugEnabled)
					_Logger.DebugFormat("transaction#{0} completed, disposing object '{1}'",
						args.Transaction.TransactionInformation.LocalIdentifier,
						instance);

				Release(instance);
			};

			return instance;
		}
	}
}