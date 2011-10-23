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

namespace Castle.Facilities.NHibernateIntegration.Tests.Issues.Facilities112
{
	using System.Reflection;
	using Core;
	using MicroKernel.Handlers;
	using MicroKernel.Lifestyle;
	using NHibernate;
	using NUnit.Framework;

	[TestFixture]
	public class LazyInitializationTestCase : IssueTestCase
	{
		protected override string ConfigurationFile
		{
			get { return "DefaultConfiguration.xml"; }
		}

		[Test]
		public virtual void SessionFactory_is_lazily_initialized()
		{
			var handler = container.Kernel.GetHandler("sessionFactory1");
			var lifestyleManagerField = typeof (DefaultHandler).GetField("lifestyleManager",
			                                                             BindingFlags.NonPublic | BindingFlags.Instance |
			                                                             BindingFlags.GetField);
			var instanceField = typeof (SingletonLifestyleManager).GetField("instance",
			                                                                BindingFlags.NonPublic | BindingFlags.Instance |
			                                                                BindingFlags.GetField);
			var lifeStyleManager = lifestyleManagerField.GetValue(handler) as SingletonLifestyleManager;
			Assert.IsNotNull(lifeStyleManager);
			var instance = instanceField.GetValue(lifeStyleManager);
			Assert.IsNull(instance);

			container.Resolve<ISessionFactory>();

			instance = instanceField.GetValue(lifeStyleManager);
			Assert.IsNotNull(instance);
		}

		[Test]
		public virtual void SessionFactory_is_singleton()
		{
			var componentModel = container.Kernel.GetHandler("sessionFactory1").ComponentModel;
			Assert.AreEqual(LifestyleType.Singleton, componentModel.LifestyleType);
		}
	}
}