// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.Remoting.CustomActivators
{
	using System.Linq;
	using System.Runtime.Remoting;
	using System.Security;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;

	/// <summary>
	/// Activates a client connecting to the remote server, enforcing the uri and the server activation.
	/// </summary>
	public class RemoteActivatorThroughConnector : DefaultComponentActivator, IDependencyAwareActivator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref = "RemoteActivatorThroughConnector" /> class.
		/// </summary>
		/// <param name = "model"> The model. </param>
		/// <param name = "kernel"> The kernel. </param>
		/// <param name = "onCreation"> The oncreation event handler. </param>
		/// <param name = "onDestruction"> The ondestruction event handler. </param>
		public RemoteActivatorThroughConnector(ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

#if !DOTNET35
		[SecuritySafeCritical]
#endif
		protected override object Instantiate(CreationContext context)
		{
			return InternalInstantiate();
		}

#if !DOTNET35
		[SecurityCritical]
#endif
		private object InternalInstantiate()
		{
			var uri = (string)Model.ExtendedProperties["remoting.uri"];
			var registry = (RemotingRegistry)Model.ExtendedProperties["remoting.remoteregistry"];

			var service = Model.Services.Single();
			if (service.IsGenericType)
			{
				registry.Publish(service);
				return RemotingServices.Connect(service, uri);
			}

			registry.Publish(Model.Name);

			return RemotingServices.Connect(service, uri);
		}

		public bool CanProvideRequiredDependencies(ComponentModel component)
		{
			return true;
		}

		public bool IsManagedExternally(ComponentModel component)
		{
			return false;
		}
	}
}