#region license

// Copyright 2011 Castle Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Diagnostics;
using System.Diagnostics.Contracts;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using log4net;

namespace Castle.Services.vNextTransaction.Lifestyles
{
	/// <summary>
	/// Abstract hybrid lifestyle manager, with two underlying lifestyles
	/// </summary>
	/// <typeparam name="M1">Primary lifestyle manager which has its constructor resolved through
	/// the main kernel.</typeparam>
	/// <typeparam name="M2">Secondary lifestyle manager</typeparam>
	public abstract class HybridLifestyleManager<M1, M2> : AbstractLifestyleManager
		where M1 : class, ILifestyleManager
		where M2 : ILifestyleManager, new()
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (HybridLifestyleManager<M1, M2>));

		private readonly IKernel _LifestyleKernel = new DefaultKernel();
		protected M1 _Lifestyle1;
		protected readonly M2 _Lifestyle2 = new M2();
		private bool _Disposed;

		[ContractPublicPropertyName("Initialized")]
		private bool _Initialized;

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

			_Logger.Debug("initializing");

			_LifestyleKernel.Register(Component.For<M1>().LifeStyle.Transient.Named("M1.lifestyle"));
			kernel.AddChildKernel(_LifestyleKernel);

			try { _Lifestyle1 = _LifestyleKernel.Resolve<M1>(); }
			finally { kernel.RemoveChildKernel(_LifestyleKernel); }

			_Lifestyle1.Init(componentActivator, kernel, model);
			_Lifestyle2.Init(componentActivator, kernel, model);

			base.Init(componentActivator, kernel, model);

			Contract.Assume(_Lifestyle1 != null,
				"lifestyle1 can't be null because the Resolve<T> call will throw an exception if a matching service wasn't found");

			_Logger.Debug("initialized");

			_Initialized = true;
		}

		public override bool Release(object instance)
		{
			bool r1 = _Lifestyle1 != null ? _Lifestyle1.Release(instance) : false;
			bool r2 = _Lifestyle2.Release(instance);
			return r1 || r2;
		}

		public override void Dispose()
		{
			if (_Disposed)
			{
				_Logger.Info("repeated call to Dispose. will show stack-trace in debug mode next. this method call is idempotent");

				if (_Logger.IsDebugEnabled)
					_Logger.Debug(new StackTrace().ToString());
			}

			if (_Lifestyle1 != null)
				_LifestyleKernel.ReleaseComponent(_Lifestyle1);

			_LifestyleKernel.Dispose();

			_Lifestyle2.Dispose();

			_Disposed = true;
		}

		public abstract override object Resolve(CreationContext context);
	}
}