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

namespace Castle.Facilities.Startable
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;

	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;

	public static class StartableFacilityRegistrationExtensions
	{
        public static ComponentRegistration<TService> Start<TService>(this ComponentRegistration<TService> registration)
            where TService : class 
		{
			return registration.AddAttributeDescriptor("startable", true.ToString());
		}

		/// <summary>
		/// Assigns the start method for the startable.
		/// </summary>
		/// <param name="registration"></param>
		/// <param name="startMethod">The start method.</param>
		/// <returns></returns>
		/// <remarks>Be sure that you first added the <see cref="Castle.Facilities.Startable.StartableFacility"/> 
		/// to the kernel, before registering this component.</remarks>
		public static ComponentRegistration<TService> StartUsingMethod<TService>(
            this ComponentRegistration<TService> registration, string startMethod)
            where TService : class 
		{
			return Start(registration)
				.AddAttributeDescriptor("startMethod", startMethod);
		}

		/// <summary>
		/// Assigns the start method for the startable.
		/// </summary>
		/// <param name="registration"></param>
		/// <param name="methodToUse">Method to use. something like: StartUsingMethod(s => s.Start)</param>
		/// <returns></returns>
		/// <remarks>Be sure that you first added the <see cref="Castle.Facilities.Startable.StartableFacility"/> 
		/// to the kernel, before registering this component.</remarks>
		public static ComponentRegistration<TService> StartUsingMethod<TService>(
            this ComponentRegistration<TService> registration, Expression<Func<TService, Action>> methodToUse)
            where TService : class 
		{
			var startMethod = ObtainMethodName(methodToUse);
			return Start(registration)
				.AddAttributeDescriptor("startMethod", startMethod);
		}

		/// <summary>
		/// Assigns the stop method for the startable.
		/// </summary>
		/// <param name="registration"></param>
		/// <param name="stopMethod">The stop method.</param>
		/// <returns></returns>
		/// <remarks>Be sure that you first added the <see cref="Castle.Facilities.Startable.StartableFacility"/> 
		/// to the kernel, before registering this component.</remarks>
		public static ComponentRegistration<TService> StopUsingMethod<TService>(
            this ComponentRegistration<TService> registration, string stopMethod)
            where TService : class 
		{
			return Start(registration)
				.AddAttributeDescriptor("stopMethod", stopMethod);
		}

		/// <summary>
		/// Assigns the stop method for the startable.
		/// </summary>
		/// <param name="registration"></param>
		/// <param name="methodToUse">Method to use. something like: StartUsingMethod(s => s.Start)</param>
		/// <returns></returns>
		/// <remarks>Be sure that you first added the <see cref="Castle.Facilities.Startable.StartableFacility"/> 
		/// to the kernel, before registering this component.</remarks>
		public static ComponentRegistration<TService> StopUsingMethod<TService>(
            this ComponentRegistration<TService> registration, Expression<Func<TService, Action>> methodToUse)
            where TService : class 
		{
			var stopMethod = ObtainMethodName(methodToUse);
			;
			return Start(registration)
				.AddAttributeDescriptor("stopMethod", stopMethod);
		}

		private static TExpression EnsureIs<TExpression>(Expression expression) where TExpression : Expression
		{
			var casted = expression as TExpression;
			if (casted == null)
			{
				throw new FacilityException(
					"Unexpected shape of expression. Expected direct call to method, something like 'x => x.Foo'");
			}

			return casted;
		}

		private static string ObtainMethodName<TService>(Expression<Func<TService, Action>> methodToUse)
		{
			var call = EnsureIs<UnaryExpression>(methodToUse.Body);
			var createDelegate = EnsureIs<MethodCallExpression>(call.Operand);
			var method = EnsureIs<ConstantExpression>(createDelegate.Arguments[2]);

			return ((MethodInfo)method.Value).Name;
		}
	}
}