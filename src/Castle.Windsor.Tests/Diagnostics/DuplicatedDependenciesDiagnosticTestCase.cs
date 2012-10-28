// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Diagnostics
{
	using System;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Diagnostics;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	public class DuplicatedDependenciesDiagnosticTestCase : AbstractContainerTestCase
	{
		private IDuplicatedDependenciesDiagnostic diagnostic;

		protected override void AfterContainerCreated()
		{
			var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
#if SILVERLIGHT
			host.AddDiagnostic<IDuplicatedDependenciesDiagnostic>(new DuplicatedDependenciesDiagnostic(Kernel));
#endif
			diagnostic = host.GetDiagnostic<IDuplicatedDependenciesDiagnostic>();
		}

		[Test]
		public void Can_detect_components_having_duplicated_dependencies_same_name_different_type()
		{
			Container.Register(Component.For<HasObjectPropertyAndTypedCtorParameterWithSameName>());

			var result = diagnostic.Inspect();
			CollectionAssert.IsNotEmpty(result);
		}

		[Test]
		public void Can_detect_components_having_duplicated_dependencies_same_type_and_name()
		{
			Container.Register(Component.For<HasTwoConstructors>());

			var result = diagnostic.Inspect();
			CollectionAssert.IsNotEmpty(result);
		}

		[Test]
		public void Can_detect_components_having_duplicated_dependencies_same_type_different_name()
		{
			Container.Register(Component.For<HasPropertyAndCtorParameterSameTypeDifferentName>());
			var result = diagnostic.Inspect();
			CollectionAssert.IsNotEmpty(result);
		}

		[Test]
		public void Can_detect_components_having_duplicated_dependencies_via_service_override()
		{
			Container.Register(Component.For<HasObjectPropertyAndTypedCtorParameterDifferentName>()
				                   .DependsOn(Dependency.OnComponent(typeof(object), typeof(EmptyService2Impl1)),
				                              Dependency.OnComponent(typeof(IEmptyService), typeof(EmptyService2Impl1))));
			var result = diagnostic.Inspect();
			CollectionAssert.IsNotEmpty(result);
		}

		[Test]
		public void member_should_action_in_context()
		{
			var types = GetType().Assembly.GetTypes().Where(t =>
			                                                {
				                                                var properties = t.GetProperties().Where(p => p.CanWrite && p.GetSetMethod() != null).ToLookup(p=>p.PropertyType);
				                                                return properties.Any(p => p.Count() > 1);
			                                                }).ToArray();
			foreach (var type in types)
			{
				Console.WriteLine(type);
			}
		}
	}
}