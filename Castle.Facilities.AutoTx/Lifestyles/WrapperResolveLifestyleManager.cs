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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using NLog;

namespace Castle.Facilities.AutoTx.Lifestyles
{
	/// <summary>
	/// 	Abstract hybrid lifestyle manager, with two underlying lifestyles
	/// </summary>
	/// <typeparam name = "T">Primary lifestyle manager which has its constructor resolved through
	/// 	the main kernel.</typeparam>
	public class WrapperResolveLifestyleManager<T> : AbstractLifestyleManager
		where T : class, ILifestyleManager
	{
		private static readonly Logger _Logger = LogManager.GetLogger(
			string.Format("Castle.Facilities.AutoTx.Lifestyles.WrapperResolveLifestyleManager<{0}>", typeof (T).Name));

		private readonly IKernel _LifestyleKernel = new DefaultKernel();
		protected T _Lifestyle1;
		private bool _Disposed;

		[ContractPublicPropertyName("Initialized")] private bool _Initialized;

		public bool Initialized
		{
			get { return _Initialized; }
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(!Initialized || _Lifestyle1 != null);
		}

		public override void Init(IComponentActivator componentActivator, IKernel kernel, ComponentModel model)
		{
			Contract.Ensures(_Lifestyle1 != null);
			Contract.Ensures(Initialized);

			if (_Logger.IsDebugEnabled)
				_Logger.Debug(() => string.Format("initializing (for component: {0})", model.Service));

			_LifestyleKernel.Register(Component.For<T>().LifeStyle.Transient.Named("T.lifestyle"));
			kernel.AddChildKernel(_LifestyleKernel);

			try
			{
				_Lifestyle1 = _LifestyleKernel.Resolve<T>();
			}
			finally
			{
				kernel.RemoveChildKernel(_LifestyleKernel);
			}

			_Lifestyle1.Init(componentActivator, kernel, model);

			base.Init(componentActivator, kernel, model);

			Contract.Assume(_Lifestyle1 != null,
			                "lifestyle1 can't be null because the Resolve<T> call will throw an exception if a matching service wasn't found");

			_Logger.Debug("initialized");

			_Initialized = true;
		}

		public override bool Release(object instance)
		{
			Contract.Requires(Initialized);
			return _Lifestyle1.Release(instance);
		}

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
			Justification = "I can't make it public and 'sealed'/non inheritable, as I'm overriding it")]
		public override void Dispose()
		{
			Contract.Ensures(!Initialized);

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool managed)
		{
			Contract.Ensures(!managed || !Initialized);

			if (!managed)
				return;

			if (_Disposed)
			{
				_Logger.Info("repeated call to Dispose. will show stack-trace in debug mode next. this method call is idempotent");

				if (_Logger.IsDebugEnabled)
					_Logger.Debug(new StackTrace().ToString());

				_Initialized = false;
				return;
			}

			try
			{
				_LifestyleKernel.ReleaseComponent(_Lifestyle1);
				_LifestyleKernel.Dispose();
				_Lifestyle1 = null;
			}
			finally
			{
				_Disposed = true;
				_Initialized = false;
			}
		}

		public override object Resolve(CreationContext context)
		{
			Contract.Requires(Initialized);
			Contract.Ensures(Contract.Result<object>() != null);
			var resolve = _Lifestyle1.Resolve(context);
			Contract.Assume(resolve != null, "the resolved instance shouldn't be null");
			return resolve;
		}
	}
}