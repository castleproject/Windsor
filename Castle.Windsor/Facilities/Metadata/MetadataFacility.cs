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
	using System.Linq;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;

	public class MetadataFacility : AbstractFacility
	{
		protected MetadataItemInterceptor GetDataInterceptor(object component)
		{
			return ((IProxyTargetAccessor)component).GetInterceptors()
			       	.Single(i => i is MetadataItemInterceptor) as MetadataItemInterceptor;
		}

		protected bool? GetShouldLoadLazilyProperty(ComponentModel componentModel)
		{
			return (bool?)componentModel.ExtendedProperties["metadata.load-lazily"];
		}

		protected override void Init()
		{
			Kernel.ComponentModelBuilder.AddContributor(new MetadataContributor());
			Kernel.ComponentCreated += PerformLazyInit;
			Kernel.Register(Component.For<MetadataDataInterceptor>().LifeStyle.Transient,
			                Component.For<MetadataItemInterceptor>().LifeStyle.Transient);
		}

		private void PerformLazyInit(ComponentModel model, object instance)
		{
			var shouldLoadLazily = GetShouldLoadLazilyProperty(model);
			if (shouldLoadLazily.HasValue == false)
			{
				return;
			}

			if (shouldLoadLazily.Value == false)
			{
				var loader = GetDataInterceptor(instance);
				loader.LoadComponent();
			}
		}
	}
}