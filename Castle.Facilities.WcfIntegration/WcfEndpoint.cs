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
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
#if DOTNET40
	using System.ServiceModel.Discovery;
#endif
	using System.Xml.Linq;
	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Internal;

	public static class WcfEndpoint
	{
		public static ServiceEndpointModel FromEndpoint(ServiceEndpoint endpoint)
		{
			return new ContractEndpointModel().FromEndpoint(endpoint);
		}

		public static ConfigurationEndpointModel FromConfiguration(string endpointName)
		{
			return new ContractEndpointModel().FromConfiguration(endpointName);
		}

		public static BindingEndpointModel BoundTo(Binding binding)
		{
			return new ContractEndpointModel().BoundTo(binding);
		}

		public static BindingAddressEndpointModel At(string address)
		{
			return new ContractEndpointModel().At(address);
		}

		public static BindingAddressEndpointModel At(Uri address)
		{
			return new ContractEndpointModel().At(address);
		}

		public static ContractEndpointModel ForContract(Type contract)
		{
			return new ContractEndpointModel(contract);
		}

		public static ContractEndpointModel ForContract<TContract>()
			where TContract : class
		{
			return ForContract(typeof(TContract));
		}

#if DOTNET40
		public static DiscoveredEndpointModel Discover()
		{
			return new ContractEndpointModel().Discover();
		}

		public static DiscoveredEndpointModel Discover(Type searchContract)
		{
			return new ContractEndpointModel().Discover(searchContract);
		}
#endif
	}

	#region Nested Class: WcfEndpointBase

	public abstract class WcfEndpointBase : IWcfEndpoint
	{
		private List<IWcfExtension> extensions;

		protected WcfEndpointBase(Type contract)
		{
			Contract = contract;
		}

		#region IWcfEndpoint Members

		public Type Contract { get; set; }

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

		void IWcfEndpoint.Accept(IWcfEndpointVisitor visitor)
		{
			Accept(visitor);
		}

		protected abstract void Accept(IWcfEndpointVisitor visitor);

		#endregion
	}

	public abstract class WcfEndpointBase<T> : WcfEndpointBase
		where T : WcfEndpointBase<T>
	{
		protected WcfEndpointBase(Type contract)
			: base(contract)
		{
		}

		public T AddExtensions(params object[] extensions)
		{
			foreach (var extension in extensions)
			{
				Extensions.Add(WcfExplicitExtension.CreateFrom(extension));
			}
			return (T)this;
		}

		public T PreserveObjectReferences()
		{
			return AddExtensions(typeof(PreserveObjectReferenceBehavior));
		}

#if DOTNET40
		#region Discovery and Metadata

		public T InScope(params Uri[] scopes)
		{
			var discovery = GetDiscoveryInstance();
			discovery.Scopes.AddAll(scopes);
			return (T)this;
		}

		public T InScope(params string[] scopes)
		{
			return InScope(scopes.Select(scope => new Uri(scope)).ToArray());
		}

		public T WithMetadata(params XElement[] metadata)
		{
			var discovery = GetDiscoveryInstance();
			discovery.Extensions.AddAll(metadata);
			return (T)this;
		}

		private EndpointDiscoveryBehavior GetDiscoveryInstance()
		{
			var discovery = Extensions.OfType<WcfInstanceExtension>()
						.Select(extension => extension.Instance)
						.OfType<EndpointDiscoveryBehavior>()
						.FirstOrDefault();

			if (discovery == null)
			{
				discovery = new EndpointDiscoveryBehavior();
				AddExtensions(WcfExplicitExtension.CreateFrom(discovery));
			}

			return discovery;
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

	#endregion

	#region Nested Class: ContractModel

	public class ContractEndpointModel : WcfEndpointBase<ContractEndpointModel>
	{
		internal ContractEndpointModel()
			: this(null)
		{
		}

		internal ContractEndpointModel(Type contract)
			: base(contract)
		{
		}

		public ServiceEndpointModel FromEndpoint(ServiceEndpoint endpoint)
		{
			if (endpoint == null)
			{
				throw new ArgumentNullException("endpoint");
			}
			return new ServiceEndpointModel(Contract, endpoint);
		}

		public ConfigurationEndpointModel FromConfiguration(string endpointName)
		{
			if (string.IsNullOrEmpty(endpointName))
			{
				throw new ArgumentException("endpointName cannot be nul or empty");
			}
			return new ConfigurationEndpointModel(Contract, endpointName);
		}

		public BindingEndpointModel BoundTo(Binding binding)
		{
			if (binding == null)
			{
				throw new ArgumentNullException("binding");
			}
			return new BindingEndpointModel(Contract, binding);
		}

		public BindingAddressEndpointModel At(string address)
		{
			return new BindingEndpointModel(Contract, null).At(address);
		}

		public BindingAddressEndpointModel At(Uri address)
		{
			return new BindingEndpointModel(Contract, null).At(address);
		}
#if DOTNET40
		public DiscoveredEndpointModel Discover()
		{
			return new BindingEndpointModel(Contract, null).Discover();
		}

		public DiscoveredEndpointModel Discover(Type searchContract)
		{
			return new BindingEndpointModel(Contract, null).Discover(searchContract);
		}
#endif
		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitContractEndpoint(this);
		}
	}

	#endregion

	#region Nested Class: ServiceEndpointModel

	public class ServiceEndpointModel : WcfEndpointBase<ServiceEndpointModel>
	{
		internal ServiceEndpointModel(Type contract, ServiceEndpoint endpoint)
			: base(contract)
		{
			ServiceEndpoint = endpoint;
		}

		public ServiceEndpoint ServiceEndpoint { get; private set; }

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitServiceEndpoint(this);
		}
	}

	#endregion

	#region Nested Class: ConfigurationEndpointModel

	public class ConfigurationEndpointModel : WcfEndpointBase<ConfigurationEndpointModel>
	{
		internal ConfigurationEndpointModel(Type contract, string endpointName)
			: base(contract)
		{
			EndpointName = endpointName;
		}

		public string EndpointName { get; private set; }

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitConfigurationEndpoint(this);
		}
	}

	#endregion

	#region Nested Class: BindingEndpointModel

	public class BindingEndpointModel : WcfEndpointBase<BindingEndpointModel>
	{
		internal BindingEndpointModel(Type contract, Binding binding)
			: base(contract)
		{
			Binding = binding;
		}

		public Binding Binding { get; private set; }

		public BindingAddressEndpointModel At(string address)
		{
			if (string.IsNullOrEmpty(address))
			{
				throw new ArgumentException("address cannot be null or empty");
			}
			return new BindingAddressEndpointModel(Contract, Binding, address);
		}

		public BindingAddressEndpointModel At(Uri address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return new BindingAddressEndpointModel(Contract, Binding, address.AbsoluteUri);
		}

		public BindingAddressEndpointModel At(EndpointAddress address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return new BindingAddressEndpointModel(Contract, Binding, address);
		}

#if DOTNET40
		public DiscoveredEndpointModel Discover()
		{
			return new DiscoveredEndpointModel(Contract, Binding, null);
		}

		public DiscoveredEndpointModel Discover(Type searchContract)
		{
			return new DiscoveredEndpointModel(Contract, Binding, searchContract);
		}
#endif

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingEndpoint(this);
		}
	}

	#endregion

	#region Nested Class: BindingAddressEndpointModel

	public class BindingAddressEndpointModel : WcfEndpointBase<BindingAddressEndpointModel>
	{
		private readonly string address;
		private readonly EndpointAddress endpointAddress;
		private string via;

		internal BindingAddressEndpointModel(Type contract, Binding binding, string address)
			: base(contract)
		{
			Binding = binding;
			this.address = address;
		}

		internal BindingAddressEndpointModel(Type contract, Binding binding, EndpointAddress address)
			: base(contract)
		{
			Binding = binding;
			endpointAddress = address;
		}

		public Binding Binding { get; private set; }

		public string Address
		{
			get { return address ?? endpointAddress.Uri.AbsoluteUri; }
		}

		public EndpointAddress EndpointAddress
		{
			get { return endpointAddress; }
		}

		public Uri ViaAddress
		{
			get { return new Uri(via, UriKind.Absolute); }
		}

		public bool HasViaAddress
		{
			get { return !string.IsNullOrEmpty(via); }
		}

		public BindingAddressEndpointModel Via(string physicalAddress)
		{
			via = physicalAddress;
			return this;
		}

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingAddressEndpoint(this);
		}
	}

	#endregion

#if DOTNET40
	#region Nested Class: DiscoveredEndpointModel

	public class DiscoveredEndpointModel : WcfEndpointBase<DiscoveredEndpointModel>
	{
		internal DiscoveredEndpointModel(Type contract, Binding binding, Type searchContract)
			: base(contract)
		{
			MaxResults = 1;
			Binding = binding;
			SearchContract = searchContract;
		}

		public int MaxResults { get; private set; }

		public Binding Binding { get; private set; }

		public bool DeriveBinding { get; private set; }

		public Type SearchContract { get; private set; }

		public Uri ScopeMatchBy { get; private set; }

		public TimeSpan? Duration { get; private set; }

		public DiscoveryEndpoint DiscoveryEndpoint { get; set; }

		public EndpointIdentity Identity { get; private set; }

		public Func<IList<EndpointDiscoveryMetadata>, EndpointDiscoveryMetadata> EndpointPreference { get; private set; }

		public DiscoveredEndpointModel Limit(int maxResults)
		{
			MaxResults = maxResults;
			return this;
		}

		public DiscoveredEndpointModel InferBinding()
		{
			DeriveBinding = true;
			return this;
		}

		public DiscoveredEndpointModel PreferEndpoint(Func<IList<EndpointDiscoveryMetadata>, EndpointDiscoveryMetadata> selector)
		{
			EndpointPreference = selector;
			return this;
		}

		public DiscoveredEndpointModel IdentifiedBy(EndpointIdentity identity)
		{
			Identity = identity;
			return this;
		}

		public DiscoveredEndpointModel With(DiscoveryEndpoint endpoint)
		{
			DiscoveryEndpoint = endpoint;
			return this;
		}

		public DiscoveredEndpointModel Span(TimeSpan duration)
		{
			Duration = duration;
			return this;
		}

		public DiscoveredEndpointModel MatchScopeExactly()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByExact);
		}

		public DiscoveredEndpointModel MatchScopeByLdap()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByLdap);
		}

		public DiscoveredEndpointModel MatchScopeByPrefix()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByPrefix);
		}

		public DiscoveredEndpointModel MatchScopeByUuid()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByUuid);
		}

		public DiscoveredEndpointModel MatchScopeBy(Uri match)
		{
			ScopeMatchBy = match;
			return this;
		}

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingDiscoveredEndpoint(this);
		}
	}

	#endregion
#endif
}

