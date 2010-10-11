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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Linq;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.Facilities.TypedFactory.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public static class TypedFactoryRegistrationExtensions
	{
		private static readonly ITypedFactoryComponentSelector defaultDelegateComponentSelector =
			new DefaultDelegateComponentSelector();

		private static readonly ITypedFactoryComponentSelector defaultInterfaceComponentSelector =
			new DefaultTypedFactoryComponentSelector();

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
		public static ComponentRegistration<TFactoryInterface> AsFactory<TFactoryInterface>(
			this ComponentRegistration<TFactoryInterface> registration)
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
		public static ComponentRegistration<TFactoryInterface> AsFactory<TFactoryInterface>(
			this ComponentRegistration<TFactoryInterface> registration, Action<TypedFactoryConfiguration> configuration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}

			if (registration.ServiceType.IsInterface)
			{
				return RegisterInterfaceBasedFactory(registration, configuration);
			}
			if (registration.ServiceType.BaseType == typeof(MulticastDelegate))
			{
				return RegisterDelegateBasedFactory(registration, configuration);
			}

			throw new ComponentRegistrationException(
				string.Format(
					"Type {0} is not an interface nor a delegate. Only interfaces and delegates may be used as typed factories.",
					registration.ServiceType));
		}

		private static ComponentRegistration<TFactory> AttachConfiguration<TFactory>(
			ComponentRegistration<TFactory> componentRegistration, Action<TypedFactoryConfiguration> configuration,
			ITypedFactoryComponentSelector defaultComponentSelector)
		{
			var factoryConfiguration = GetFactoryConfiguration(configuration);
			var selectorReference = factoryConfiguration.Reference;
			if (selectorReference == null)
			{
				return componentRegistration.DynamicParameters((k, d) =>
				{
					var selector = defaultComponentSelector;
					d.Insert(selector);
				});
			}
			return componentRegistration.DynamicParameters((k, c, d) =>
			{
				var selector = selectorReference.Resolve(k, c);
				d.Insert(selector);
				return k2 => k2.ReleaseComponent(selector);
			});
		}

		private static ComponentRegistration<T> AttachDelegateFactory<T>(ComponentRegistration<T> registration)
		{
			return registration.UsingFactoryMethod((k, m, c) =>
			{
				var delegateProxyFactory = k.Resolve<IProxyFactoryExtension>(TypedFactoryFacility.DelegateProxyFactoryKey,
				                                                             new Arguments()
				                                                             	.Insert("targetDelegateType", registration.ServiceType));
				var @delegate = k.ProxyFactory.Create(delegateProxyFactory, k, m, c);
				k.ReleaseComponent(delegateProxyFactory);
				return (T)@delegate;
			});
		}

		private static ComponentRegistration<TFactory> AttachFactoryInterceptor<TFactory>(
			ComponentRegistration<TFactory> registration)
		{
			return registration.Interceptors(new InterceptorReference(TypedFactoryFacility.InterceptorKey)).Last;
		}

		private static TypedFactoryConfiguration GetFactoryConfiguration(Action<TypedFactoryConfiguration> configuration)
		{
			var factoryConfiguration = new TypedFactoryConfiguration();

			if (configuration != null)
			{
				configuration.Invoke(factoryConfiguration);
			}
			return factoryConfiguration;
		}

		private static bool HasOutArguments(Type serviceType)
		{
			return serviceType.GetMethods().Any(m => m.GetParameters().Any(p => p.IsOut));
		}

		private static ComponentRegistration<TDelegate> RegisterDelegateBasedFactory<TDelegate>(
			ComponentRegistration<TDelegate> registration, Action<TypedFactoryConfiguration> configuration)
		{
			if (HasOutArguments(registration.ServiceType))
			{
				throw new ComponentRegistrationException(
					string.Format("Delegate type {0} can not be used as typed factory because it has 'out' arguments.",
					              registration.ServiceType));
			}
			var invoke = DelegateFactory.ExtractInvokeMethod(registration.ServiceType);
			if (invoke == null)
			{
				throw new ComponentRegistrationException(
					string.Format("Delegate type {0} can not be used as typed factory because it has void return type.",
					              registration.ServiceType));
			}
			var settings = new TypedFactoryConfiguration();
			if (configuration != null)
			{
				configuration.Invoke(settings);
			}

			var componentRegistration = AttachFactoryInterceptor(registration);
			componentRegistration = AttachDelegateFactory(componentRegistration);
			return AttachConfiguration(componentRegistration, configuration, defaultDelegateComponentSelector);
		}

		private static ComponentRegistration<TFactoryInterface> RegisterInterfaceBasedFactory<TFactoryInterface>(
			ComponentRegistration<TFactoryInterface> registration, Action<TypedFactoryConfiguration> configuration)
		{
			if (HasOutArguments(registration.ServiceType))
			{
				throw new ComponentRegistrationException(
					string.Format("Type {0} can not be used as typed factory because it has methods with 'out' arguments.",
					              registration.ServiceType));
			}
			var componentRegistration = AttachFactoryInterceptor(registration);
			return AttachConfiguration(componentRegistration, configuration, defaultInterfaceComponentSelector);
		}
	}
}