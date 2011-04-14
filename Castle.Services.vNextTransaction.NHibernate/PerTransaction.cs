using System;
using System.Diagnostics.Contracts;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using log4net;

namespace Castle.Services.vNextTransaction.NHibernate
{
	[Serializable]
	public class PerTransaction : AbstractLifestyleManager
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (PerTransaction));

		private readonly ITxManager _Manager;

		public PerTransaction(ITxManager manager)
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
			_Manager.CurrentTopTransaction.Value.Inner.TransactionCompleted += (sender, args) => Release(instance);
			return instance;
		}
	}
}