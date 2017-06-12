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
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.XmlFiles;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class PropertiesInspectionBehaviorTestCase
	{
		[Test]
		public void InvalidOption()
		{
			var expectedMessage = "Error on properties inspection. Could not convert the inspectionBehavior attribute value into an expected enum value. Value found is 'Invalid' while possible values are 'Undefined, None, All, DeclaredOnly'";
			var exception = Assert.Throws<ConverterException>(() =>
				new WindsorContainer(new XmlInterpreter(Xml.Embedded("propertyInspectionBehaviorInvalid.xml"))));
			Assert.AreEqual(exception.Message, expectedMessage);
		}

		[Test]
		public void PropertiesInspectionTestCase()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("propertyInspectionBehavior.xml")));

			var comp = container.Resolve<ExtendedComponentWithProperties>("comp1");
			Assert.IsNull(comp.Prop1);
			Assert.AreEqual(0, comp.Prop2);
			Assert.AreEqual(0, comp.Prop3);

			comp = container.Resolve<ExtendedComponentWithProperties>("comp2"); // All
			Assert.IsNotNull(comp.Prop1);
			Assert.AreEqual(1, comp.Prop2);
			Assert.AreEqual(2, comp.Prop3);

			comp = container.Resolve<ExtendedComponentWithProperties>("comp3"); // DeclaredOnly
			Assert.IsNull(comp.Prop1);
			Assert.AreEqual(0, comp.Prop2);
			Assert.AreEqual(2, comp.Prop3);
		}
	}
}
