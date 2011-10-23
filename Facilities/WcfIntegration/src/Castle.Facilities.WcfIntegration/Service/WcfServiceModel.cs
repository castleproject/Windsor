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
	using System.Linq;
    using System.Collections.Generic;
	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Internal;

    public abstract class WcfServiceModelBase : IWcfServiceModel
    {
        private ICollection<Uri> baseAddresses;
		private ICollection<IWcfEndpoint> endpoints;
		private ICollection<IWcfExtension> extensions;

		#region IWcfServiceModel 

		public bool IsHosted { get; protected set; }

		public bool? ShouldOpenEagerly { get; protected set; }

		public ICollection<Uri> BaseAddresses
		{
			get
			{
				if (baseAddresses == null)
				{
					baseAddresses = new List<Uri>();
				}
				return baseAddresses;
			}
			set { baseAddresses = value; }
		}

		public ICollection<IWcfEndpoint> Endpoints
		{
			get
			{
				if (endpoints == null)
				{
					endpoints = new List<IWcfEndpoint>();
				}
				return endpoints;
			}
			set { endpoints = value; }
		}

		public ICollection<IWcfExtension> Extensions
		{
			get
			{
				if (extensions == null)
				{
					extensions = new List<IWcfExtension>();
				}
				return extensions;
			}
		}

		#endregion
	}

	public abstract class WcfServiceModel<T> : WcfServiceModelBase
		where T : WcfServiceModel<T>
	{
		protected WcfServiceModel()
		{
		}

		protected WcfServiceModel(IWcfEndpoint endpoint)
		{
			AddEndpoints(endpoint);
		}

		public T Hosted()
		{
			IsHosted = true;
			return (T)this;
		}

		public T OpenEagerly()
		{
			ShouldOpenEagerly = true;
			return (T)this;
		}

		public T AddBaseAddresses(params Uri[] baseAddresses)
		{
			BaseAddresses.AddAll(baseAddresses);
			return (T)this;
		}

		public T AddBaseAddresses(params string[] baseAddresses)
		{
			return AddBaseAddresses(baseAddresses.Select(a => new Uri(a, UriKind.Absolute)).ToArray());
		}

		public T AddEndpoints(params IWcfEndpoint[] endpoints)
		{
			Endpoints.AddAll(endpoints);
			return (T)this;
		}

		public T AddExtensions(params object[] extensions)
		{
			Extensions.AddAll(extensions.Select(extension => WcfExplicitExtension.CreateFrom(extension)));
			return (T)this;
		}

		#region Metadata

		public T PublishMetadata()
		{
			return AddExtensions(new WcfMetadataExtension());
		}

		public T PublishMetadata(Action<WcfMetadataExtension> mex)
		{
			var mexExtension = new WcfMetadataExtension();
			if (mex != null) mex(mexExtension);
			return AddExtensions(mexExtension);
		}

		public T ProvideMetadata<TMeta>() where TMeta : IWcfMetadataProvider
		{
			return AddExtensions(typeof(TMeta));
		}

		public T ProvideMetadata(Type metaProvider)
		{
			if (typeof(IWcfMetadataProvider).IsAssignableFrom(metaProvider) == false)
			{
				throw new ArgumentException(string.Format("The metaProvider {0} does not implement {1}.",
					metaProvider, typeof(IWcfMetadataProvider)));
			}
			return AddExtensions(metaProvider);
		}

		public T ProviderMetadata(IWcfMetadataProvider provider)
		{
			return AddExtensions(provider);
		}

		#endregion

#if DOTNET40
		#region Discovery

		public T Discoverable()
		{
			return AddExtensions(new WcfDiscoveryExtension());
		}

		public T Discoverable(Action<WcfDiscoveryExtension> discover)
		{
			var discoveryExtension = new WcfDiscoveryExtension();
			if (discover != null) discover(discoveryExtension);
			return AddExtensions(discoveryExtension);
		}

		#endregion
#endif

		#region Logging

		public T LogMessages()
		{
			return AddExtensions(typeof(LogMessageEndpointBehavior));
		}

		public T LogMessages<F>()
			where F : IFormatProvider, new()
		{
			return LogMessages<F>(null);
		}

		public T LogMessages<F>(string format)
			where F : IFormatProvider, new()
		{
			return LogMessages(new F(), format);
		}

		public T LogMessages(IFormatProvider formatter)
		{
			return LogMessages(formatter, null);
		}

		public T LogMessages(IFormatProvider formatter, string format)
		{
			return LogMessages().AddExtensions(new LogMessageFormat(formatter, format));
		}

		public T LogMessages(string format)
		{
			return LogMessages().AddExtensions(new LogMessageFormat(format));
		}

		#endregion
	}
}