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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public static class TypedFactoryRegistrationExtensions
	{
		/// <summary>
		/// Marks the component as typed factory.
		/// </summary>
		/// <typeparam name="S"></typeparam>
		/// <param name="registration"></param>
		/// <returns></returns>
		/// <remarks>
		/// Only interfaces are legal to use as typed factories. Methods with out parameters are not allowed.
		/// When registering component as typed factory no implementation should be provided (in case there is any it will be ignored).
		/// Typed factories rely on <see cref="IInterceptorSelector"/> set internally, so users should not set interceptor selectors explicitly;
		/// otherwise the factory will not function correctly.
		/// </remarks>
		public static ComponentRegistration<S> AsFactory<S>(this ComponentRegistration<S> registration)
		{
			if( registration == null )
			{
				throw new ArgumentNullException( "registration" );
			}

			if (registration.ServiceType.IsInterface == false)
			{
				throw new ComponentRegistrationException(
					string.Format("Type {0} is not an interface. Only interfaces may be used as typed factories.",
					              registration.ServiceType));
			}

			if(HasOutArguments(registration.ServiceType))
			{
				throw new ComponentRegistrationException(
					string.Format("Type {0} can not be used as typed factory because it has methods with 'out' arguments.",
					              registration.ServiceType));
			}

			return registration.Interceptors( new InterceptorReference( TypedFactoryFacility.InterceptorKey ) ).Last;

		}

		private static bool HasOutArguments(Type serviceType)
		{
			return serviceType.GetMethods().Any(m => m.GetParameters().Any(p => p.IsOut));
		}
	}
}