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

namespace Castle.MicroKernel.Tests.Registration.Interceptors
{
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Registration.Interceptors.Multiple;
	using Castle.MicroKernel.Tests.Registration.Interceptors.Single;

	using NUnit.Framework;

	[TestFixture]
	public sealed class InterceptorsTestCase : RegistrationTestCaseBase
	{
		[Test]
		public void GenericInterceptorsInSingleCall()
		{
			ExecuteScenario<GenericInterceptorsInSingleCall>();
		}
		
		[Test]
		public void InterceptorKeyInSingleCall()
		{
			ExecuteScenario<InterceptorKeyInSingleCall>();
		}

		[Test]
		public void GenericInterceptorsMultipleCall()
		{
			ExecuteScenario<GenericInterceptorsMultipleCall>();
		}

		[Test]
		public void InterceptorKeyMultipleCall()
		{
			ExecuteScenario<InterceptorKeyMultipleCall>();
		}

		[Test]
		public void InterceptorReferenceAnywhereMultipleCall()
		{
			ExecuteScenario<InterceptorReferenceAnywhereMultipleCall>();
		}

		[Test]
		public void InterceptorReferencesAnywhereInSingleCall()
		{
			ExecuteScenario<InterceptorReferencesWithPositionInSingleCall>();
		}

		[Test]
		public void InterceptorReferencesWithPositionInSingleCall1()
		{
			ExecuteScenario<InterceptorReferencesWithPositionInSingleCall1>();
		}

		[Test]
		public void InterceptorReferencesWithPositionInSingleCall2()
		{
			ExecuteScenario<InterceptorReferencesWithPositionInSingleCall2>();
		}

		[Test]
		public void InterceptorReferencesWithPositionInSingleCall3()
		{
			ExecuteScenario<InterceptorReferencesWithPositionInSingleCall3>();
		}

		[Test]
		public void InterceptorReferenceWithPositionMultipleCall1()
		{
			ExecuteScenario<InterceptorReferenceWithPositionMultipleCall1>();
		}

		[Test]
		public void InterceptorReferenceWithPositionMultipleCall2()
		{
			ExecuteScenario<InterceptorReferenceWithPositionMultipleCall2>();
		}

		[Test]
		public void InterceptorReferenceWithPositionMultipleCall3()
		{
			ExecuteScenario<InterceptorReferenceWithPositionMultipleCall3>();
		}

		[Test]
		public void InterceptorTypeMultipleCall()
		{
			ExecuteScenario<InterceptorTypeMultipleCall>();
		}

		[Test]
		public void GenericInterceptor()
		{
			ExecuteScenario<SingleGenericInterceptor>();
		}

		[Test]
		public void SingleInterceptorReference()
		{
			ExecuteScenario<SingleInterceptorReference>();
		}

		[Test]
		public void SingleInterceptorKey()
		{
			ExecuteScenario<SingleInterceptorKey>();
		}

		[Test]
		public void SingleInterceptorType()
		{
			ExecuteScenario<SingleInterceptorType>();
		}

		[Test]
		public void InterceptorTypesInSingleCall()
		{
			ExecuteScenario<InterceptorTypesInSingleCall>();
		}

		
		private void ExecuteScenario<TScenario>() where TScenario : InterceptorsTestCaseHelper, new()
		{
			var scenario = new TScenario();
			var registration = Component.For<ICustomer>();

			scenario.RegisterInterceptors(registration);

			Kernel.Register(registration);

			var handler = Kernel.GetHandler(typeof(ICustomer));

			AssertInterceptorReferencesAreEqual(handler, scenario);
		}

		private void AssertInterceptorReferencesAreEqual(IHandler handler, InterceptorsTestCaseHelper helper)
		{
			CollectionAssert.AreEqual(helper.GetExpectedInterceptorsInCorrectOrder(),
			                          handler.ComponentModel.Interceptors);
		}
	}
}
