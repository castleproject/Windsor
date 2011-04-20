using System;
using System.Diagnostics.Contracts;
using Castle.Windsor;
using log4net;

namespace Castle.Facilities.AutoTx.Testing
{
	public class ResolveScope<T> : IDisposable
		where T : class
	{
		private static readonly ILog _Logger = LogManager.GetLogger(
			string.Format("Castle.Facilities.AutoTx.Testing.ResolveScope<{0}>", typeof(T).Name));

		private readonly T _Service;
		private bool _Disposed;
		protected readonly IWindsorContainer _Container;

		public ResolveScope(IWindsorContainer container)
		{
			Contract.Requires(container != null);
			Contract.Ensures(_Service != null, "or resolve throws");

			_Logger.Debug("creating");

			_Container = container;
			_Service = _Container.Resolve<T>();
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Service != null);
		}

		public virtual T Service
		{
			get
			{
				Contract.Ensures(Contract.Result<T>() != null);
				return _Service;
			}
		}

		public virtual void Dispose()
		{
			if (_Disposed) return;

			_Logger.Debug("disposing resolve scope");

			try
			{
				_Container.Release(_Service);
			}
			finally
			{
				_Disposed = true;
			}
		}
	}
}