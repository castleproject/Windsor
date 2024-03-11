
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace Castle.Windsor.Extensions.DependencyInjection
{
	/// <summary>
	/// Microsoft Dependency Injection with keyed registeration uses keys
	/// of type object, while castle has only string keys, we need to generate
	/// a new key for each registration to fix this impedance mismatch
	/// </summary>
	internal class KeyedRegistrationHelper
	{
		/// <summary>
		/// We need to keep tracks of all registration of a given key, we can have more than one service registered with a specific
		/// key so when the caller want to resolve with a key we can know which service to resolve. Internally we generate a unique
		/// string to register the object inside castle so we can easily resolve by name.
		/// </summary>
		/// <param name="Key">The key used to register the service</param>
		/// <param name="ServiceDescriptor">Original Service Descriptor used to perform the registration</param>
		/// <param name="CastleRegistrationKey">The name used inside castle to register the service.</param>
		/// <param name="ServiceKeyParameterName">If the constructor has one parameter with the framework attribute ServiceKey we are
		/// saving into this property the name of the parameter</param>
		internal record KeyedRegistration(
			object Key,
			ServiceDescriptor ServiceDescriptor,
			string CastleRegistrationKey,
			string ServiceKeyParameterName)
		{
			/// <summary>
			/// Resolve using the current key for castle.
			/// </summary>
			/// <param name="container">Container used to resolve</param>
			/// <param name="currentKey">The current key used to resolve the service, this happens because if you use the generic
			/// key we need to use the one used for the resolution. In can be null, in this scenario we will use the original
			/// key used for the registration</param>
			/// <returns></returns>
			internal object Resolve(IWindsorContainer container, object currentKey = null)
			{
				//Support the parameter in constructor that want key to be injected
				if (ServiceKeyParameterName != null)
				{
					var argumentParameters = new Dictionary<string, object>()
					{
						[ServiceKeyParameterName] = currentKey ?? Key
					};
					return container.Resolve(CastleRegistrationKey, ServiceDescriptor.ServiceType, argumentParameters);
				}
				return container.Resolve(CastleRegistrationKey, ServiceDescriptor.ServiceType);
			}
		}

		/// <summary>
		/// For each key we can have more than one service registered, this allows us to resolve the correct service. Also you can
		/// resolve all the interfaces registered with a specific key.
		/// </summary>
		private readonly ConcurrentDictionary<object, List<KeyedRegistration>> _keyToRegistrationMap = new();

		private readonly ConcurrentDictionary<Type, List<KeyedRegistration>> _typeToRegistrationMap = new();

		private static readonly ConcurrentDictionary<IWindsorContainer, KeyedRegistrationHelper> _keyedRegistrations = new();

		internal static KeyedRegistrationHelper GetInstance(IWindsorContainer container)
		{
			if (!_keyedRegistrations.TryGetValue(container, out var helper))
			{
				helper = new KeyedRegistrationHelper();
				_keyedRegistrations.TryAdd(container, helper);
			}

			return helper;
		}

		internal const string KeyedRegistrationPrefix = "__KEYEDSERVICE__";

		/// <summary>
		/// Framework has special key KeyedService.AnyKey that is used to register a service with any key. This means that
		/// whenever we are resolving a service with a key that is not registered, this can be used as fallback. So we 
		/// need to explicitly know which services are registered with any key.
		/// </summary>
		private readonly ConcurrentDictionary<Type, KeyedRegistration> _typeRegisteredWithAnyKey = new();

#if NET8_0_OR_GREATER

		public string GetOrCreateKey(object key, ServiceDescriptor serviceDescriptor)
		{
			ArgumentNullException.ThrowIfNull(key);

			var registrationKey = $"{KeyedRegistrationPrefix}+{Guid.NewGuid():N}";
			var registration = CreateRegistration(key, serviceDescriptor, registrationKey);
			if (!_keyToRegistrationMap.TryGetValue(key, out var registrations))
			{
				registrations = new List<KeyedRegistration>
				{
					registration
				};
				_keyToRegistrationMap.AddOrUpdate(key, registrations, (_, v) => { v.Add(registration); return v; });
			}
			else
			{
				//ok we already have the concurrent bag.
				registrations.Add(registration);
			}

			//Now create an inverse map, where we have all registration done for a specific type.
			if (!_typeToRegistrationMap.TryGetValue(serviceDescriptor.ServiceType, out var keyList))
			{
				keyList = new List<KeyedRegistration>
				{
					registration
				};
				_typeToRegistrationMap.AddOrUpdate(serviceDescriptor.ServiceType, keyList, (_, v) => { v.Add(registration); return v; });
			}
			else
			{
				keyList.Add(registration);
			}

			if (key == KeyedService.AnyKey)
			{
				var registered = _typeRegisteredWithAnyKey.TryAdd(serviceDescriptor.ServiceType, registration);
				if (!registered)
				{
					throw new NotSupportedException("Cannot register more than one instance of the same service with the anykey.");
				}
			}

			return registration.CastleRegistrationKey;
		}

		/// <summary>
		/// There is a special attribute called ServiceKeyAttribute in the framework that require DI system to resolve
		/// the service and pass the key object to that specific parameter. It seems an unnecessary feature but it
		/// is tested by the standard test suite.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="serviceDescriptor"></param>
		/// <param name="registrationKey"></param>
		/// <returns></returns>
		private static KeyedRegistration CreateRegistration(object key, ServiceDescriptor serviceDescriptor, string registrationKey)
		{
			string serviceKeyParameterName = null;
			if (serviceDescriptor.KeyedImplementationType != null)
			{
				var constructors = serviceDescriptor.KeyedImplementationType.GetConstructors();
				foreach (var constructor in constructors)
				{
					var parameters = constructor.GetParameters();
					foreach (var parameter in parameters)
					{
						if (parameter.GetCustomAttributes(typeof(ServiceKeyAttribute), true).Length != 0)
						{
							serviceKeyParameterName = parameter.Name;
							break;
						}
					}
				}
			}

			return new KeyedRegistration(key, serviceDescriptor, registrationKey, serviceKeyParameterName);
		}

		public KeyedRegistration GetKey(object key, Type serviceType)
		{
			ArgumentNullException.ThrowIfNull(key);
			ArgumentNullException.ThrowIfNull(serviceType);

			if (_keyToRegistrationMap.TryGetValue(key, out var registrations))
			{
				//ok we have services for this key.
				var registration = registrations.FirstOrDefault(r => r.ServiceDescriptor.ServiceType == serviceType);
				return registration;
			}

			//ok is it possible that this service was registered with any key?
			if (_typeRegisteredWithAnyKey.TryGetValue(serviceType, out var registrationWithAnyKey))
			{
				//ok we really 
				return registrationWithAnyKey;
			}

			return null;
		}

		public IEnumerable<KeyedRegistration> GetKeyedRegistrations(Type serviceType)
		{
			ArgumentNullException.ThrowIfNull(serviceType);

			if (_typeToRegistrationMap.TryGetValue(serviceType, out var registrations))
			{
				return registrations;
			}

			return Array.Empty<KeyedRegistration>();
		}

		public bool HasKey(object key)
		{
			ArgumentNullException.ThrowIfNull(key);

			return _keyToRegistrationMap.ContainsKey(key);
		}
#endif
	}
}