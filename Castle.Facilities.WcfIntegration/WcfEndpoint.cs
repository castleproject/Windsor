// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Discovery;
	using System.Xml.Linq;
	using Castle.Facilities.WcfIntegration.Behaviors;

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

		public static ContractEndpointModel ForContract<Contract>()
			where Contract : class
		{
			return ForContract(typeof(Contract));
		}

		public static DiscoveredEndpointModel Discover()
		{
			return new ContractEndpointModel().Discover();
		}
	}

	#region Nested Class: WcfEndpointBase

	public abstract class WcfEndpointBase : IWcfEndpoint
	{
		private ICollection<IWcfExtension> extensions;

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
			foreach (object extension in extensions)
			{
				Extensions.Add(WcfExplicitExtension.CreateFrom(extension));
			}
			return (T)this;
		}

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

		public DiscoveredEndpointModel Discover()
		{
			return new BindingEndpointModel(Contract, null).Discover();
		}

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

		public DiscoveredEndpointModel Discover()
		{
			return new DiscoveredEndpointModel(Contract, Binding);
		}

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

	#region Nested Class: DiscoveredEndpointModel

	public class DiscoveredEndpointModel : WcfEndpointBase<DiscoveredEndpointModel>
	{
		private Uri _scopeMatchBy;
		private readonly List<Uri> _scopes;
		private readonly List<XElement> _filters;

		internal DiscoveredEndpointModel(Type contract, Binding binding)
			: base(contract)
		{
			Binding = binding;
			_scopes = new List<Uri>();
			_filters = new List<XElement>();
		}

		public Binding Binding { get; private set; }

		public Uri ScopeMatchBy
		{
			get { return _scopeMatchBy; }
		}

		public IEnumerable<Uri> Scopes
		{
			get { return _scopes; }
		}

		public IEnumerable<XElement> Filters
		{
			get { return _filters; }
		}

		public TimeSpan? Duration { get; private set; }

		public DiscoveryEndpoint DiscoveryEndpoint { get; set; }

		public DiscoveredEndpointModel With(DiscoveryEndpoint endpoint)
		{
			DiscoveryEndpoint = endpoint;
			return this;
		}

		public DiscoveredEndpointModel SearchFor(TimeSpan duration)
		{
			Duration = duration;
			return this;
		}

		#region Scopes

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
			_scopeMatchBy = match;
			return this;
		}

		public DiscoveredEndpointModel InScope(params Uri[] scopes)
		{
			_scopes.AddRange(scopes);
			return this;
		}

		#endregion

		public DiscoveredEndpointModel FilteredBy(params XElement[] filters)
		{
			_filters.AddRange(filters);
			return this;
		}

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingDiscoveredEndpoint(this);
		}
	}

	#endregion
}

