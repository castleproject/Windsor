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

namespace Castle.Facilities.NHibernateIntegration.Tests.Issues.Facilities119
{
	using NHibernate.Cfg;
	using NUnit.Framework;

	[TestFixture]
	public class Fixture : IssueTestCase
	{
		protected override void ExportDatabaseSchema()
		{
		}

		protected override void DropDatabaseSchema()
		{
		}

		[Test]
		public void Configurations_can_be_obtained_via_different_ConfigurationBuilders()
		{
			var configuration1 = container.Resolve<Configuration>("sessionFactory1.cfg");
			var configuration2 = container.Resolve<Configuration>("sessionFactory2.cfg");
			var configuration3 = container.Resolve<Configuration>("sessionFactory3.cfg");
			Assert.AreEqual(configuration1.GetProperty("test"), "test1");
			Assert.AreEqual(configuration2.GetProperty("test"), "test2");
			Assert.AreEqual(configuration3.GetProperty("test"), "test3");
		}
	}
}