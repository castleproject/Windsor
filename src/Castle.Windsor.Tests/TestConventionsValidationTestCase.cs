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

namespace CastleTests
{
	using System;
	using System.Linq;
	using System.Reflection;

	using NUnit.Framework;
	
	[Explicit]
	public class TestConventionsValidationTestCase : AbstractContainerTestCase
	{
		[Test]
		public void All_Test_Cases_should_be_named_something_TestCase()
		{
			var types = GetType().GetTypeInfo().Assembly.GetExportedTypes();
			var testCases = types.Where(t => t.GetTypeInfo().IsDefined(typeof(TestFixtureAttribute), inherit: true))
				.Except(new[] { typeof(AbstractContainerTestCase) })
				.ToArray();

			var illNamedTestCases = testCases.Where(t => t.Name.EndsWith("TestCase") == false).ToArray();
			Assert.IsEmpty(illNamedTestCases, string.Join<Type>(Environment.NewLine, illNamedTestCases));
		}

		[Test]
		public void All_Test_Cases_should_inherit_AbstractContainerTestCase()
		{
			var types = GetType().GetTypeInfo().Assembly.GetExportedTypes();
			var testCases = types.Where(t => t.GetTypeInfo().IsDefined(typeof(TestFixtureAttribute), inherit: true))
				.Except(new[] { typeof(AbstractContainerTestCase) })
				.ToArray();

			var missingTestCases = testCases.Where(t => typeof(AbstractContainerTestCase).GetTypeInfo().IsAssignableFrom(t) == false).ToArray();
			Assert.IsEmpty(missingTestCases, string.Join<Type>(Environment.NewLine, missingTestCases));
		}
	}
}