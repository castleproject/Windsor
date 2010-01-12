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

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111
{
#if (!SILVERLIGHT)
	using Castle.Core.Resource;
	using Castle.Windsor.Configuration.Interpreters;

	using NUnit.Framework;

	[TestFixture]
	public class FACILITIES_ISSUE_111
	{
		private IResource setupResource;

		[SetUp]
		public void Setup()
		{
			setupResource = new StaticContentResource(@"<?xml version=""1.0"" encoding=""utf-8"" ?>

<configuration>

    <facilities>
      <facility id=""startable"" type=""Castle.Facilities.Startable.StartableFacility, Castle.MicroKernel"" />
    </facilities>

	<components>

		<!--if this line is uncommented (and the serviceB declared below commented), serviceA is resolved correctly -->
		<!--<component id=""ServiceB"" type=""StartableFacilityTest.B, StartableFacilityTest"" service=""StartableFacilityTest.IB, StartableFacilityTest"" />-->

		<component id=""ServiceA"" type=""Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components.A_Facilities_Issue_111, Castle.Windsor.Tests"" service=""Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components.IA_Facilities_Issue_111, Castle.Windsor.Tests"">
			<parameters>
				<ibs>
					<array>
						<item>${ServiceB}</item>
					</array>
				</ibs>
			</parameters>
		</component>

		<component id=""ServiceB"" type=""Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components.B_Facilities_Issue_111, Castle.Windsor.Tests"" service=""Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components.IB_Facilities_Issue_111, Castle.Windsor.Tests"" />

	</components>

</configuration>
");

		}

		[Test]
		public void Registering_IStartable_Out_Of_Order_On_Array_Should_Not_Throw_Exception()
		{
			new WindsorContainer(new XmlInterpreter(setupResource));
		}

	}
#endif
}
