// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.Synchronize
{
	using System.Threading;
	using System.Windows.Forms;
	using System.Windows.Threading;

	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;

	/// <summary>
	/// Augments the kernel to handle synchronized components.
	/// </summary>
	public class SynchronizeFacility : AbstractFacility
	{
		/// <summary>
		/// Registers all components needed by the facility.
		/// </summary>
		protected override void Init()
		{
			Kernel.Register(
				Component.For<SynchronizeInterceptor>(),
				Component.For<SynchronizeMetaInfoStore>()
				);

			Kernel.ComponentModelBuilder.AddContributor(new SynchronizeComponentInspector(Kernel));
			Kernel.ComponentModelBuilder.AddContributor(new CreateOnUIThreadInspector(Kernel, FacilityConfig));
			RegisterAmbientSynchronizationContext<WindowsFormsSynchronizationContext>();
			RegisterAmbientSynchronizationContext<DispatcherSynchronizationContext>();
		}

		private void RegisterAmbientSynchronizationContext<TContext>()
			where TContext : SynchronizationContext
		{
			var syncContext = SynchronizationContext.Current as TContext;
			if (syncContext != null)
			{
				Kernel.Register(Component.For<TContext>().Instance(syncContext));
			}
		}
	}
}