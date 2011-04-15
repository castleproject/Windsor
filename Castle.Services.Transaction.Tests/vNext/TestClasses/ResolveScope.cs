using System;
using Castle.Windsor;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class ResolveScope<T> : IDisposable
	{
		private readonly IWindsorContainer _Container;

		private readonly T _Service;

		public ResolveScope(IWindsorContainer container)
		{
			_Container = container;
			_Service = _Container.Resolve<T>();
		}

		public T Service
		{
			get { return _Service; }
		}

		public void Dispose()
		{
			_Container.Release(_Service);
		}
	}
}