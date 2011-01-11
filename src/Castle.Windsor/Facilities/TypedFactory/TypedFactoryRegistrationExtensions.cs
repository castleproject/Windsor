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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.Facilities.TypedFactory.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public static class TypedFactoryRegistrationExtensions
	{
		/// <summary>
		///   Marks the component as typed factory.
		/// </summary>
		/// <typeparam name = "TFactoryInterface"></typeparam>
		/// <param name = "registration"></param>
		/// <returns></returns>
		/// <remarks>
		///   Only interfaces are legal to use as typed factories. Methods with out parameters are not allowed.
		///   When registering component as typed factory no implementation should be provided (in case there is any it will be ignored).
		///   Typed factories rely on <see cref = "IInterceptorSelector" /> set internally, so users should not set interceptor selectors explicitly;
		///   otherwise the factory will not function correctly.
		/// </remarks>
		public static ComponentRegistration<TFactoryInterface> AsFactory<TFactoryInterface>(this ComponentRegistration<TFactoryInterface> registration)
		{
			return AsFactory(registration, null);
		}

		/// <summary>
		///   Marks the component as typed factory.
		/// </summary>
		/// <typeparam name = "TFactoryInterface"></typeparam>
		/// <param name = "registration"></param>
		/// <param name = "configuration"></param>
		/// <returns></returns>
		/// <remarks>
		///   Only interfaces are legal to use as typed factories. Methods with out parameters are not allowed.
		///   When registering component as typed factory no implementation should be provided (in case there is any it will be ignored).
		///   Typed factories rely on <see cref = "IInterceptorSelector" /> set internally, so users should not set interceptor selectors explicitly;
		///   otherwise the factory will not function correctly.
		/// </remarks>
		public static ComponentRegistration<TFactoryInterface> AsFactory<TFactoryInterface>(this ComponentRegistration<TFactoryInterface> registration,
		                                                                                    Action<TypedFactoryConfiguration> configuration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			var classServices = registration.Services.TakeWhile(s => s.IsClass).ToArray();
			if (classServices.Length == 0)
			{
				Debug.Assert(registration.Services.Count > 0, "registration.services.Count > 0");
				return RegisterInterfaceBasedFactory(registration, configuration);
			}
			if (classServices.Length != 1)
			{
				throw new ComponentRegistrationException(
					"This component can not be used as typed factory because it exposes more than one class service. Only component exposing single depegate, or interfaces can be used as typed factories.");
			}
			var classService = classServices[0];
			if (classService.BaseType == typeof(MulticastDelegate))
			{
				if (registration.Services.Count == 1)
				{
					return RegisterDelegateBasedFactory(registration, configuration);
				}
				throw new ComponentRegistrationException(
					string.Format(
						"Type {0} is a delegate, however the component has also {1} inteface(s) specified as it's service. Delegate-based typed factories can't expose any additional services.",
						classService, registration.Services.Count - 1));
			}

			throw new ComponentRegistrationException(
				string.Format(
					"Type {0} is not an interface nor a delegate. Only interfaces and delegates may be used as typed factories.",
					classService));
		}

		private static ComponentRegistration<TFactory> AttachConfiguration<TFactory>(ComponentRegistration<TFactory> componentRegistration,
		                                                                             Action<TypedFactoryConfiguration> configuration,
		                                                                             string defaultComponentSelectorKey)
		{
			var selectorReference = GetSelectorReference(configuration, defaultComponentSelectorKey);
			componentRegistration.AddDescriptor(new ReferenceDependencyDescriptor<TFactory>(selectorReference));
			if (IsAnyServiceOpenGeneric(componentRegistration))
			{
				componentRegistration.AddAttributeDescriptor(TypedFactoryFacility.IsFactoryKey, "true");
			}
			else
			{
				componentRegistration.AddDescriptor(new FactoryCacheDescriptor<TFactory>());
			}
			return componentRegistration.DynamicParameters((k, c, d) =>
			{
				var selector = selectorReference.Resolve(k, c);
				d.Insert(selector);
				return k2 => k2.ReleaseComponent(selector);
			});
		}

		private static ComponentRegistration<TFactory> AttachFactoryInterceptor<TFactory>(ComponentRegistration<TFactory> registration)
		{
			return registration.Interceptors(new InterceptorReference(TypedFactoryFacility.InterceptorKey)).Last;
		}

		private static IReference<ITypedFactoryComponentSelector> GetSelectorReference(Action<TypedFactoryConfiguration> configuration,
		                                                                               string defaultComponentSelectorKey)
		{
			var factoryConfiguration = new TypedFactoryConfiguration(defaultComponentSelectorKey);

			if (configuration != null)
			{
				configuration.Invoke(factoryConfiguration);
			}
			return factoryConfiguration.Reference;
		}

		private static bool HasOutArguments(Type serviceType)
		{
			return serviceType.GetMethods().Any(m => m.GetParameters().Any(p => p.IsOut));
		}

		private static bool IsAnyServiceOpenGeneric<TFactory>(ComponentRegistration<TFactory> componentRegistration)
		{
			if (componentRegistration.Services.Any(i => i.ContainsGenericParameters))
			{
				return true;
			}
			return false;
		}

		private static ComponentRegistration<TDelegate> RegisterDelegateBasedFactory<TDelegate>(ComponentRegistration<TDelegate> registration,
		                                                                                        Action<TypedFactoryConfiguration> configuration)
		{
			var delegateType = registration.Services.Single();
			if (HasOutArguments(delegateType))
			{
				throw new ComponentRegistrationException(
					string.Format("Delegate type {0} can not be used as typed factory because it has 'out' arguments.",
					              delegateType));
			}
			var invoke = DelegateFactory.ExtractInvokeMethod(delegateType);
			if (invoke == null)
			{
				throw new ComponentRegistrationException(
					string.Format("Delegate type {0} can not be used as typed factory because it has void return type.",
					              delegateType));
			}
			var settings = new TypedFactoryConfiguration(TypedFactoryFacility.DefaultDelegateSelectorKey);
			if (configuration != null)
			{
				configuration.Invoke(settings);
			}

			var componentRegistration = AttachFactoryInterceptor(registration);
			componentRegistration.UsingFactoryMethod((k, m, c) =>
			{
				var delegateProxyFactory = k.Resolve<IProxyFactoryExtension>(TypedFactoryFacility.DelegateProxyFactoryKey,
				                                                             new Arguments()
				                                                             	.Insert("targetDelegateType", delegateType));
				var @delegate = k.ProxyFactory.Create(delegateProxyFactory, k, m, c);
				k.ReleaseComponent(delegateProxyFactory);
				return (TDelegate)@delegate;
			});
			return AttachConfiguration(componentRegistration, configuration, TypedFactoryFacility.DefaultDelegateSelectorKey);
		}

		private static ComponentRegistration<TFactoryInterface> RegisterInterfaceBasedFactory<TFactoryInterface>(
			ComponentRegistration<TFactoryInterface> registration, Action<TypedFactoryConfiguration> configuration)
		{
			foreach (var serviceType in registration.Services)
			{
				if (HasOutArguments(serviceType))
				{
					throw new ComponentRegistrationException(
						string.Format("Type {0} can not be used as typed factory because it has methods with 'out' arguments.", serviceType));
				}
			}
			var componentRegistration = AttachFactoryInterceptor(registration);
			return AttachConfiguration(componentRegistration, configuration, TypedFactoryFacility.DefaultInterfaceSelectorKey);
		}
	}
}