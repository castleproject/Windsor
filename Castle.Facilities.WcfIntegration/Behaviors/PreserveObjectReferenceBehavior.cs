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
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;
	using System.Xml;

	interface IPreserveObjectReferences
	{
		int MaxItemsInObjectGraph { get; }
		bool IgnoreExtensionDataObject { get; }
	}

	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public class PreserveObjectReferenceBehavior : Attribute, IContractBehavior, IPreserveObjectReferences
	{
		public PreserveObjectReferenceBehavior()
			: this(0xFFFF, false)
		{
		}

		public PreserveObjectReferenceBehavior(int maxItemsInObjectGraph, bool ignoreExtensionDataObject)
		{
			MaxItemsInObjectGraph = maxItemsInObjectGraph;
			IgnoreExtensionDataObject = ignoreExtensionDataObject;
		}

		public int MaxItemsInObjectGraph { get; private set; }

		public bool IgnoreExtensionDataObject { get; private set; }

		void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			PreserveObjectReferenceSerializerOperationBehavior.ReplaceDataContractSerializer(endpoint.Contract, this);
		}

		void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
		{
			PreserveObjectReferenceSerializerOperationBehavior.ReplaceDataContractSerializer(endpoint.Contract, this);
		}

		void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
		{
		}
	}

	#region Class PreserveObjectReferenceSerializerOperationBehavior

	public class PreserveObjectReferenceSerializerOperationBehavior : DataContractSerializerOperationBehavior
	{
		public PreserveObjectReferenceSerializerOperationBehavior(
			OperationDescription operationDescription, int maxItemsInObjectGraph, bool ignoreExtensionDataObject)
			: base(operationDescription)
		{
			MaxItemsInObjectGraph = maxItemsInObjectGraph;
			IgnoreExtensionDataObject = ignoreExtensionDataObject;
		}

		public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
		{
			return new DataContractSerializer(type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, true, null);
		}

		public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
		{
			return new DataContractSerializer(type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, true, null);
		}

		internal static void ReplaceDataContractSerializer(ContractDescription contractDescription, IPreserveObjectReferences preserverObjectReferences)
		{
			foreach (var operation in contractDescription.Operations)
			{
				ReplaceDataContractSerializer(operation, preserverObjectReferences);
			}
		}

		internal static void ReplaceDataContractSerializer(OperationDescription operation, IPreserveObjectReferences preserverObjectReference)
		{
			if (operation.Behaviors.Remove(typeof(DataContractSerializerOperationBehavior)) ||
				operation.Behaviors.Remove(typeof(PreserveObjectReferenceSerializerOperationBehavior)))
			{
				operation.Behaviors.Add(new PreserveObjectReferenceSerializerOperationBehavior(operation,
					preserverObjectReference.MaxItemsInObjectGraph, preserverObjectReference.IgnoreExtensionDataObject));
			}
		}
	}

	#endregion

	#region Class PreserveObjectReference

	[AttributeUsage(AttributeTargets.Method)]
	public class PreserveObjectReference : Attribute, IOperationBehavior, IPreserveObjectReferences
	{
		public PreserveObjectReference() : this(0xFFFF, false)
		{
		}

		public PreserveObjectReference(int maxItemsInObjectGraph, bool ignoreExtensionDataObject)
		{
			MaxItemsInObjectGraph = maxItemsInObjectGraph;
			IgnoreExtensionDataObject = ignoreExtensionDataObject;
		}

		public int MaxItemsInObjectGraph { get; private set; }

		public bool IgnoreExtensionDataObject { get; private set; }

		public void ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
		{
			PreserveObjectReferenceSerializerOperationBehavior.ReplaceDataContractSerializer(description, this);
		}

		public void ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
		{
			PreserveObjectReferenceSerializerOperationBehavior.ReplaceDataContractSerializer(description, this);
		}

		public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
		{
		}

		public void Validate(OperationDescription description)
		{
		}
	}

	#endregion
}
