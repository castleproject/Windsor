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

namespace Castle.Facilities.WcfIntegration
{
	using System.ServiceModel;

	/// <summary>
	/// Abstarct implementation of <see cref="IServiceHostAware"/>
	/// </summary>
	public abstract class AbstractServiceHostAware : IServiceHostAware
	{
		void IServiceHostAware.Created(ServiceHost serviceHost)
		{
			Created(serviceHost);
		}

		protected virtual void Created(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Opening(ServiceHost serviceHost)
		{
			Opening(serviceHost);
		}

		protected virtual void Opening(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Opened(ServiceHost serviceHost)
		{
			Opened(serviceHost);
		}

		protected virtual void Opened(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Closing(ServiceHost serviceHost)
		{
			Closing(serviceHost);
		}

		protected virtual void Closing(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Closed(ServiceHost serviceHost)
		{
			Closed(serviceHost);
		}

		protected virtual void Closed(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Faulted(ServiceHost serviceHost)
		{
			Faulted(serviceHost);
		}

		protected virtual void Faulted(ServiceHost serviceHost)
		{
		}
	}
}
