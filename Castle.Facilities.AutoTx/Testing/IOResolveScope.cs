using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using Castle.Windsor;

namespace Castle.Facilities.AutoTx.Testing
{
	/// <summary>
	/// A resolve scope where T is the service you wish to get from the container.
	/// </summary>
	/// <typeparam name="T">The service to resolve.</typeparam>
	public class IOResolveScope<T> : ResolveScope<T>
		where T : class
	{
		private readonly IDirectoryAdapter _Dir;
		private readonly IFileAdapter _File;
		private readonly ITxManager _Manager;
		private bool _Disposed;

		public IOResolveScope(IWindsorContainer container) : base(container)
		{
			_Dir = _Container.Resolve<IDirectoryAdapter>();
			Contract.Assume(_Dir != null, "resolve throws otherwise");

			_File = _Container.Resolve<IFileAdapter>();
			Contract.Assume(_File != null, "resolve throws otherwise");

			_Manager = _Container.Resolve<ITxManager>();
			Contract.Assume(_Manager != null);
		}

		/// <summary>
		/// Gets the directory adapter.
		/// </summary>
		public IDirectoryAdapter Directory
		{
			get { return _Dir; }
		}

		/// <summary>
		/// Gets the file adapter.
		/// </summary>
		public IFileAdapter File
		{
			get { return _File; }
		}

		public ITxManager Manager
		{
			get { return _Manager; }
		}

		public override T Service
		{
			get { return base.Service; }
		}

		public override void Dispose()
		{
			if (_Disposed) return;

			try
			{
				_Container.Release(_Dir);
				_Container.Release(_File);
			}
			finally
			{
				_Disposed = true;
				base.Dispose();
			}
		}
	}
}