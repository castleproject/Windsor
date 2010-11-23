#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion
namespace Castle.Facilities.NHibernateIntegration.Tests
{
	using Core.Resource;
	using MicroKernel.Facilities;
	using NUnit.Framework;
	using Windsor;
	using Windsor.Configuration.Interpreters;

	[TestFixture]
	public class FacilityFluentConfigTestCase
	{
		[Test]
		public void Should_override_DefaultConfigurationBuilder()
		{
			var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

			var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

			container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.CustomConfigurationBuilder = typeof(TestConfigurationBuilder));

			Assert.AreEqual(typeof(TestConfigurationBuilder), container.Resolve<IConfigurationBuilder>().GetType());
		}

		[Test, ExpectedException(typeof(FacilityException))]
		public void Should_not_accept_non_implementors_of_IConfigurationBuilder_for_override()
		{
			var file = "Castle.Facilities.NHibernateIntegration.Tests/MinimalConfiguration.xml";

			var container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(file)));

			container.AddFacility<NHibernateFacility>("nhibernatefacility", f => f.CustomConfigurationBuilder = GetType());
		}
	}
}
