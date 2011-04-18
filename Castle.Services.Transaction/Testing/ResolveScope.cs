using System;
using System.Diagnostics.Contracts;
using Castle.Windsor;
using log4net;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class ResolveScope<T> : IDisposable
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ResolveScope<T>));

		private readonly IWindsorContainer _Container;

		private readonly T _Service;

		public ResolveScope(IWindsorContainer container)
		{
			Contract.Requires(container != null);

			_Logger.Debug("creating");

			_Container = container;
			_Service = _Container.Resolve<T>();
		}

		public T Service
		{
			get
			{
				Contract.Ensures(Contract.Result<T>() != null);
				return _Service;
			}
		}

		public void Dispose()
		{
			_Logger.Debug("disposing");
			_Container.Release(_Service);
		}
	}
}