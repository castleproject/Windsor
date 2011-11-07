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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.ServiceModel.Channels;

	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	/// <summary>
	///   Facility to simplify the management of WCF clients and services.
	/// </summary>
	public class WcfFacility : AbstractFacility
	{
		private readonly WcfClientExtension clientExtension = new WcfClientExtension();
		private readonly WcfServiceExtension serviceExtension = new WcfServiceExtension();

		public WcfClientExtension Clients
		{
			get { return clientExtension; }
		}

		public TimeSpan? CloseTimeout { get; set; }

		public Binding DefaultBinding { get; set; }

		public WcfServiceExtension Services
		{
			get { return serviceExtension; }
		}

		protected override void Dispose()
		{
			base.Dispose();

			clientExtension.Dispose();
			serviceExtension.Dispose();
		}

		protected override void Init()
		{
			clientExtension.Init(Kernel, this);
			serviceExtension.Init(Kernel, this);

			Kernel.Register(
				Component.For<WcfClientExtension>().Instance(clientExtension),
				Component.For<WcfServiceExtension>().Instance(serviceExtension),
				Component.For<ILazyComponentLoader>().ImplementedBy<WcfClientComponentLoader>()
				);

			Kernel.ComponentModelBuilder.AddContributor(new WcfBehaviorInspector());
		}
	}
}