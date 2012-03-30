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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.Runtime.Serialization;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;

	public class DataContractSurrogateBehavior : Attribute, IContractBehavior
	{
		private readonly IDataContractSurrogate surrogate;

		public DataContractSurrogateBehavior(Type surrogateType)
		{
			if (typeof(IDataContractSurrogate).IsAssignableFrom(surrogateType) == false)
			{
				throw new ArgumentException(string.Format(
					"The data contract surrogate {0} does not implement {1}.",
					surrogateType.FullName,
					typeof(IDataContractSurrogate).FullName),
					"surrogateType");
			}

			var surrogateCtor = surrogateType.GetConstructor(Type.EmptyTypes);
			if (surrogateCtor == null)
			{
				throw new ArgumentException(string.Format(
					"The data contract surrogate {0} does not have an empty public constructor.",
					surrogateType.FullName));
			}

			surrogate = (IDataContractSurrogate)surrogateCtor.Invoke(new object[0]);
		}

		public DataContractSurrogateBehavior(IDataContractSurrogate surrogate)
		{
			this.surrogate = surrogate;
		}

		void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			ConfigureDataContractSurrogate(endpoint.Contract);
		}

		void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
		{
			ConfigureDataContractSurrogate(endpoint.Contract);
		}

		void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
		{
		}

		private void ConfigureDataContractSurrogate(ContractDescription contractDescription)
		{
			foreach (var operation in contractDescription.Operations)
			{
				var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
				serializer.DataContractSurrogate = surrogate;
			}
		}
	}
}
