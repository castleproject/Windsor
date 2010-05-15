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

namespace Castle.Facilities.Metadata
{
	using System;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.Registration;

	public class MetadataContributor : IContributeComponentModelConstruction
	{
		private readonly IInterceptorSelector selector = new MetaComponentInterceptorSelector();

		private Type BuildMetaType(Type metaDataType, ComponentModel componentModel)
		{
			return typeof(IMeta<,>).MakeGenericType(componentModel.Service, metaDataType);
		}

		private bool HasMetadata(ComponentModel componentModel)
		{
			return componentModel.ExtendedProperties.Contains("metadata.metadata-type") &&
			       componentModel.ExtendedProperties.Contains("metadata.metadata-adapter");
		}

		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (HasMetadata(model) == false)
			{
				return;
			}

			var metaDataType = (Type)model.ExtendedProperties["metadata.metadata-type"];
			var metaDataAdapter = model.ExtendedProperties["metadata.metadata-adapter"];
			var loadLazily = model.ExtendedProperties["metadata.load-lazily"] ?? false;
			var metaType = BuildMetaType(metaDataType, model);
			var registration = Component.For(metaType).Interceptors<MetadataDataInterceptor, MetadataItemInterceptor>()
				.SelectInterceptorsWith(selector)
				.ExtendedProperties(Property.ForKey("metadata.metadata-adapter").Eq(metaDataAdapter),
				                    Property.ForKey("metadata.item-name").Eq(model.Name),
				                    Property.ForKey("metadata.load-lazily").Eq(loadLazily));

			if (model.LifestyleType == LifestyleType.Custom)
			{
				kernel.Register(registration.LifeStyle.Custom(model.CustomComponentActivator));
			}
			else
			{
				kernel.Register(registration.LifeStyle.Is(model.LifestyleType));
			}
		}
	}
}