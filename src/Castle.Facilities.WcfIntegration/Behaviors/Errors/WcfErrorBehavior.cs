﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;

	public class WcfErrorBehavior : IServiceBehavior, IEndpointBehavior
	{
		private readonly ICollection<IErrorHandler> errorHandlers;

		public WcfErrorBehavior(ICollection<IErrorHandler> errorHandlers)
		{
			this.errorHandlers = errorHandlers;
		}

		public WcfErrorBehavior(params IErrorHandler[] errorHandlers)
		{
			this.errorHandlers = new List<IErrorHandler>(errorHandlers);
		}

		public void Add(IEnumerable<IErrorHandler> handlers)
		{
			foreach (var handler in handlers)
			{
				errorHandlers.Add(handler);
			}
		}

		public void Add(IErrorHandler handler)
		{
			errorHandlers.Add(handler);
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			foreach (var errorHandler in errorHandlers)
			{
				clientRuntime.CallbackDispatchRuntime.ChannelDispatcher.ErrorHandlers.Add(errorHandler);
			}
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			foreach (var errorHandler in errorHandlers)
			{
				endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(errorHandler);
			}
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
		                                 Collection<ServiceEndpoint> endpoints,
		                                 BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
			{
				foreach (var errorHandler in errorHandlers)
				{
					channelDispatcher.ErrorHandlers.Add(errorHandler);
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}