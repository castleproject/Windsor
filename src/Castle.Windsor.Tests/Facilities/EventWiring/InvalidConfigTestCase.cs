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


#if (!SILVERLIGHT)

namespace CastleTests.Facilities.EventWiring
{
	using Castle.Facilities.EventWiring;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests;
	using Castle.XmlFiles;

	using NUnit.Framework;

	[TestFixture]
	public class InvalidConfigTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.Install(Configuration.FromXml(Xml.Embedded("EventWiringFacility/invalid.config")));
		}

		[Test]
		[ExpectedException(typeof(EventWiringException))]
		public void InvalidConfigured()
		{
			Container.Resolve<object>("SimplePublisher");
		}
	}
}

#endif