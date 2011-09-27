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

using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using Castle.Windsor;

namespace Castle.Facilities.AutoTx.Testing
{
	public class ResolveScope<T> : IDisposable
		where T : class
	{
		private readonly ILogger _Logger;

		private readonly T _Service;
		private bool _Disposed;
		protected readonly IWindsorContainer Container;

		public ResolveScope(IWindsorContainer container)
		{
			Contract.Requires(container != null);
			Contract.Ensures(_Service != null, "or resolve throws");

			// check container has a logger factory component
			ILoggerFactory loggerFactory = container.GetService<ILoggerFactory>();
			if (loggerFactory != null)
			{
				// create logger
				_Logger = loggerFactory.Create(GetType());
			}
			else
				_Logger = NullLogger.Instance;

			if(_Logger.IsDebugEnabled)
				_Logger.Debug("creating");

			Container = container;
			_Service = Container.Resolve<T>();
			Contract.Assume(_Service != null, "by resolve<T>");
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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool managed)
		{
			if (_Disposed) return;

			if(_Logger.IsDebugEnabled)
				_Logger.Debug("disposing resolve scope");

			try
			{
				Container.Release(_Service);
			}
			finally
			{
				_Disposed = true;
			}
		}
	}
}