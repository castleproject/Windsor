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
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;

	public class FactoryActivator : DefaultComponentActivator, IDependencyAwareActivator
	{
		public FactoryActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		public bool CanProvideRequiredDependencies(ComponentModel component)
		{
			return true;
		}

		public bool IsManagedExternally(ComponentModel component)
		{
			return false;
		}

		protected override object Instantiate(CreationContext context)
		{
			var factoryId = (String)Model.ExtendedProperties["factoryId"];
			var factoryCreate = (String)Model.ExtendedProperties["factoryCreate"];

			var factoryHandler = Kernel.GetHandler(factoryId);
			if (factoryHandler == null)
			{
				var message = String.Format("You have specified a factory ('{2}') " +
				                            "for the component '{0}' {1} but the kernel does not have this " +
				                            "factory registered",
				                            Model.Name, Model.Implementation.FullName, factoryId);
				throw new FacilityException(message);
			}

			// Let's find out whether the create method is a static or instance method

			var factoryType = factoryHandler.ComponentModel.Implementation;

			var staticCreateMethod =
				factoryType.GetMethod(factoryCreate,
				                      BindingFlags.Public | BindingFlags.Static);

			if (staticCreateMethod != null)
			{
				return Create(null, factoryId, staticCreateMethod, factoryCreate, context);
			}
			var factoryInstance = Kernel.Resolve<object>(factoryId);

			var instanceCreateMethod =
				factoryInstance.GetType().GetMethod(factoryCreate,
				                                    BindingFlags.Public | BindingFlags.Instance);

			if (instanceCreateMethod == null)
			{
				factoryInstance = ProxyUtil.GetUnproxiedInstance(factoryInstance);

				instanceCreateMethod =
					factoryInstance.GetType().GetMethod(factoryCreate,
					                                    BindingFlags.Public | BindingFlags.Instance);
			}

			if (instanceCreateMethod != null)
			{
				return Create(factoryInstance, factoryId, instanceCreateMethod, factoryCreate, context);
			}
			else
			{
				var message = String.Format("You have specified a factory " +
				                            "('{2}' - method to be called: {3}) " +
				                            "for the component '{0}' {1} but we couldn't find the creation method" +
				                            "(neither instance or static method with the name '{3}')",
				                            Model.Name, Model.Implementation.FullName, factoryId, factoryCreate);
				throw new FacilityException(message);
			}
		}

		private object Create(object factoryInstance, string factoryId, MethodInfo instanceCreateMethod, string factoryCreate, CreationContext context)
		{
			object instance;
			var methodArgs = new List<object>();
			try
			{
				var parameters = instanceCreateMethod.GetParameters();

				foreach (var parameter in parameters)
				{
					var paramType = parameter.ParameterType;

					if (paramType == typeof(IKernel))
					{
						methodArgs.Add(Kernel);
						continue;
					}
					else if (paramType == typeof(CreationContext))
					{
						methodArgs.Add(context);
						continue;
					}

					var dependency = new DependencyModel(parameter.Name, paramType, false);
					dependency.Init(Model.HasParameters ? Model.Parameters : null);
					if (!Kernel.Resolver.CanResolve(context, null, Model, dependency))
					{
						var message = String.Format(
							"Factory Method {0}.{1} requires an argument '{2}' that could not be resolved",
							instanceCreateMethod.DeclaringType.FullName, instanceCreateMethod.Name, parameter.Name);
						throw new FacilityException(message);
					}

					var arg = Kernel.Resolver.Resolve(context, null, Model, dependency);

					methodArgs.Add(arg);
				}

				instance = instanceCreateMethod.Invoke(factoryInstance, methodArgs.ToArray());
			}
			catch (Exception ex)
			{
				var message = String.Format("You have specified a factory " +
				                            "('{2}' - method to be called: {3}) " +
				                            "for the component '{0}' {1} that failed during invoke.",
				                            Model.Name, Model.Implementation.FullName, factoryId, factoryCreate);

				throw new FacilityException(message, ex);
			}

			if (Model.HasInterceptors)
			{
				try
				{
					instance = Kernel.ProxyFactory.Create(Kernel, instance, Model, context, methodArgs.ToArray());
				}
				catch (Exception ex)
				{
					throw new ComponentActivatorException("FactoryActivator: could not proxy " +
					                                      instance.GetType().FullName, ex);
				}
			}

			return instance;
		}
	}
}