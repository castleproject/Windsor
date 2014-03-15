// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.Startable
{
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor;

	public partial class StartableFacility : AbstractFacility
	{
		private ITypeConverter converter;
		private StartFlag flag;

		/// <summary>
		///     This method changes behavior of the facility. Deferred mode should be used when you have single call to <see cref = "IWindsorContainer.Install" /> and register all your components there. Enabling
		///     this mode will optimize the behavior of the facility so that it will wait 'till the end of installation and only after all <see cref = "IWindsorInstaller" />s were ran it will instantiate and
		///     start all the startable components. An exception will be thrown if a startable component can't be instantiated and started. This will help you fail fast and diagnose issues quickly. If you don't
		///     want the exception to be thrown and you prefer the component to fail silently, use <see cref = "DeferredTryStart" /> method instead.
		/// </summary>
		/// <remarks>It is recommended to use this method over <see cref = "DeferredTryStart" /> method.</remarks>
		public void DeferredStart()
		{
			DeferredStart(new DeferredStartFlag());
		}

		/// <summary>
		///     Startable components will be started when <see cref = "StartFlag.Signal" /> method is invoked. This is particularily usedul when you need to perform some extra initialization outside of container
		///     before starting the Startable components.
		/// </summary>
		public void DeferredStart(StartFlag flag)
		{
			this.flag = flag;
		}

		/// <summary>
		///     This method changes behavior of the facility. Deferred mode should be used when you have single call to <see cref = "IWindsorContainer.Install" /> and register all your components there. Enabling
		///     this mode will optimize the behavior of the facility so that it will wait 'till the end of installation and only after all <see cref = "IWindsorInstaller" />s were ran it will instantiate and
		///     start all the startable components. No exception will be thrown if a startable component can't be instantiated and started. If you'd rather fail fast and diagnose issues quickly, use
		///     <see cref = "DeferredStart()" /> method instead.
		/// </summary>
		/// <remarks>It is recommended to use <see cref = "DeferredStart()" /> method over this method.</remarks>
		public void DeferredTryStart()
		{
			DeferredStart(new DeferredTryStartFlag());
		}

		protected override void Init()
		{
			converter = Kernel.GetConversionManager();
			Kernel.ComponentModelBuilder.AddContributor(new StartableContributor(converter));

			InitFlag(flag ?? new LegacyStartFlag(), new StartableEvents(Kernel));
		}

		private void InitFlag(IStartFlagInternal startFlag, StartableEvents events)
		{
			startFlag.Init(events);
		}

		public static bool IsStartable(IHandler handler)
		{
			var startable = handler.ComponentModel.ExtendedProperties["startable"];
			var isStartable = (bool?)startable;
			return isStartable.GetValueOrDefault();
		}
	}
}