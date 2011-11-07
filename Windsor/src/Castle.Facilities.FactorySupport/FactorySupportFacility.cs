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

namespace Castle.Facilities.FactorySupport
{
	using System;
	using System.ComponentModel;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	using Component = Castle.MicroKernel.Registration.Component;

	public class FactorySupportFacility : AbstractFacility
	{
		[Obsolete("Use 'UsingFactoryMethod' method in fluent registration API")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddAccessor<TService, TFactory>(string serviceKey, string instanceAccessorMethod)
		{
			AddAccessor<TService, TFactory>(serviceKey, instanceAccessorMethod, typeof(TFactory).FullName);
		}

		[Obsolete("Use 'UsingFactoryMethod' method in fluent registration API")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddAccessor<TService, TFactory>(string serviceKey, string instanceAccessorMethod, string factoryId)
		{
			IConfiguration cfg = new MutableConfiguration(serviceKey);
			cfg.Attributes["instance-accessor"] = instanceAccessorMethod;

			AddFactoryComponent<TService, TFactory>(cfg, factoryId, serviceKey);
		}

		[Obsolete("Use 'UsingFactoryMethod' method in fluent registration API")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddFactory<TService, TFactory>(string serviceKey, string factoryCreateMethodName)
		{
			AddFactory<TService, TFactory>(serviceKey, factoryCreateMethodName, typeof(TFactory).FullName);
		}

		[Obsolete("Use 'UsingFactoryMethod' method in fluent registration API")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddFactory<TService, TFactory>(string serviceKey, string factoryCreateMethodName, string factoryId)
		{
			var cfg = new MutableConfiguration(serviceKey);
			cfg.Attributes["factoryCreate"] = factoryCreateMethodName;
			AddFactoryComponent<TService, TFactory>(cfg, factoryId, serviceKey);
		}

		protected override void Init()
		{
			Kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
		}

		private void AddFactoryComponent<TService, TFactory>(IConfiguration cfg, string factoryId, string serviceKey)
		{
			var factoryType = typeof(TFactory);
			var serviceType = typeof(TService);

			EnsureFactoryIsRegistered(factoryId, factoryType);

			var serviceModel = Kernel.ComponentModelBuilder.BuildModel(new ComponentName(serviceKey, true), new[] { serviceType }, factoryType, null);
			cfg.Attributes["factoryId"] = factoryId;
			serviceModel.Configuration = cfg;
			((IKernelInternal)Kernel).AddCustomComponent(serviceModel);
		}

		private void EnsureFactoryIsRegistered(string factoryId, Type factoryType)
		{
			if (!Kernel.HasComponent(factoryType))
			{
				Kernel.Register(Component.For(factoryType).Named(factoryId));
			}
		}

		private void Kernel_ComponentModelCreated(ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return;
			}

			var instanceAccessor = model.Configuration.Attributes["instance-accessor"];
			var factoryId = model.Configuration.Attributes["factoryId"];
			var factoryCreate = model.Configuration.Attributes["factoryCreate"];

			if ((factoryId != null && factoryCreate == null) ||
			    (factoryId == null && factoryCreate != null))
			{
				var message = String.Format("When a factoryId is specified, you must specify " +
				                            "the factoryCreate (which is the method to be called) as well - component {0}",
				                            model.Name);

				throw new FacilityException(message);
			}

			if (instanceAccessor != null)
			{
				model.ExtendedProperties.Add("instance.accessor", instanceAccessor);
				model.CustomComponentActivator = typeof(AccessorActivator);
			}
			else if (factoryId != null)
			{
				model.ExtendedProperties.Add("factoryId", factoryId);
				model.ExtendedProperties.Add("factoryCreate", factoryCreate);
				model.CustomComponentActivator = typeof(FactoryActivator);
			}
		}
	}
}