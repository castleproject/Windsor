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

namespace Castle.MicroKernel.Tests.Registration.Interceptors.Multiple
{
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Interceptors;

	public class InterceptorReferenceAnywhereMultipleCall : InterceptorsTestCaseHelper
	{
		public override IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration)
		{
			return registration.Interceptors(new InterceptorReference(typeof(TestInterceptor1))).Anywhere
				.Interceptors(new InterceptorReference(typeof(TestInterceptor2))).Anywhere;
		}

		public override IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder()
		{
			yield return InterceptorReference.ForType<TestInterceptor1>();
			yield return InterceptorReference.ForType<TestInterceptor2>();
		}
	}
}
