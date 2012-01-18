﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration.Behaviors
{
	using System;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;
	using Castle.Core.Logging;

	public class LogMessageEndpointBehavior : AbstractExtensibleObject<LogMessageEndpointBehavior>, IEndpointBehavior
	{
		private readonly IExtendedLoggerFactory loggerFactory;

		public LogMessageEndpointBehavior(IExtendedLoggerFactory loggerFactory)
		{
			this.loggerFactory = loggerFactory;
		}

		public IFormatProvider MessageFormatter { get; set; }

		public string DefaultMessageFormat { get; set; }

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			clientRuntime.MessageInspectors.Add(CreateLogMessageInspector(endpoint.Contract.ContractType));
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			var serviceType = endpointDispatcher.ChannelDispatcher.Host.Description.ServiceType;
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(CreateLogMessageInspector(serviceType));
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}

		private LogMessageInspector CreateLogMessageInspector(Type serviceType)
		{
			string format = null;
			IFormatProvider formatter = null;
			var logger = loggerFactory.Create(serviceType);

			var formatBehavior = Extensions.Find<LogMessageFormat>();
			if (formatBehavior != null)
			{
				format = formatBehavior.MessageFormat;
				formatter = formatBehavior.FormatProvider;
			}

			format = format ?? DefaultMessageFormat ?? string.Empty;
			formatter = formatter ?? MessageFormatter ?? CustomMessageFormatter.Instance;

			return new LogMessageInspector(logger, formatter, format);
		}
	}
}
