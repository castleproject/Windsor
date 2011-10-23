#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

#endregion

using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using Castle.Windsor;

namespace Castle.Facilities.AutoTx.Testing
{
	/// <summary>
	/// 	A resolve scope where T is the service you wish to get from the container.
	/// </summary>
	/// <typeparam name = "T">The service to resolve.</typeparam>
	public class IOResolveScope<T> : ResolveScope<T>
		where T : class
	{
		private readonly IDirectoryAdapter _Dir;
		private readonly IFileAdapter _File;
		private readonly ITransactionManager _Manager;
		private bool _Disposed;

		public IOResolveScope(IWindsorContainer container) : base(container)
		{
			Contract.Requires(container != null, "container mustn't be null");

			_Dir = Container.Resolve<IDirectoryAdapter>();
			Contract.Assume(_Dir != null, "resolve throws otherwise");

			_File = Container.Resolve<IFileAdapter>();
			Contract.Assume(_File != null, "resolve throws otherwise");

			_Manager = Container.Resolve<ITransactionManager>();
			Contract.Assume(_Manager != null, "resolve throws otherwise");
		}

		/// <summary>
		/// 	Gets the directory adapter.
		/// </summary>
		public IDirectoryAdapter Directory
		{
			get { return _Dir; }
		}

		/// <summary>
		/// 	Gets the file adapter.
		/// </summary>
		public IFileAdapter File
		{
			get { return _File; }
		}

		public ITransactionManager Manager
		{
			get { return _Manager; }
		}

		public override T Service
		{
			get { return base.Service; }
		}

		protected override void Dispose(bool managed)
		{
			if (_Disposed) return;

			try
			{
				Container.Release(_Dir);
				Container.Release(_File);
			}
			finally
			{
				_Disposed = true;
				base.Dispose(true);
			}
		}
	}
}