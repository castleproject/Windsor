// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.Registration.Interceptors
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	public abstract class InterceptorsTestCaseBase : RegistrationTestCaseBase
	{
		protected abstract IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration);
		protected abstract IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder();

		[Test]
		public void AddComponent_WithInterceptors_ComponentModelShouldHaveRightInterceptors()
		{
			var registration = Component.For<ICustomer>();

			RegisterInterceptors(registration);

			Kernel.Register(registration);

			var handler = Kernel.GetHandler(typeof(ICustomer));

			AssertInterceptorReferencesAreEqual(handler);
		}

		private void AssertInterceptorReferencesAreEqual(IHandler handler)
		{
			CollectionAssert.AreEqual(GetExpectedInterceptorsInCorrectOrder(),
									  handler.ComponentModel.Interceptors.Cast<InterceptorReference>(),
									  new InterceptorReferenceComparer());
		}
	}
}
