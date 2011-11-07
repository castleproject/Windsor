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
	using NHibernate.Cfg;
	using NHibernate.Tool.hbm2ddl;
	using NUnit.Framework;
	using Rhino.Mocks;
	using Windsor;
	using Windsor.Configuration.Interpreters;

	public abstract class AbstractNHibernateTestCase
	{
		protected IWindsorContainer container;
		protected MockRepository mockRepository;

		public AbstractNHibernateTestCase()
		{
			mockRepository = new MockRepository();
		}

		protected virtual string ConfigurationFile
		{
			get { return "DefaultConfiguration.xml"; }
		}

		protected virtual void ExportDatabaseSchema()
		{
			Configuration[] cfgs = container.ResolveAll<Configuration>();
			foreach (Configuration cfg in cfgs)
			{
				SchemaExport export = new SchemaExport(cfg);
				export.Create(false, true);
			}
		}

		protected virtual void DropDatabaseSchema()
		{
			Configuration[] cfgs = container.ResolveAll<Configuration>();
			foreach (Configuration cfg in cfgs)
			{
				SchemaExport export = new SchemaExport(cfg);
				export.Drop(false, true);
			}
		}

		[SetUp]
		public virtual void SetUp()
		{
			container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(GetContainerFile())));
			ConfigureContainer();
			ExportDatabaseSchema();
			OnSetUp();
		}

		[TearDown]
		public virtual void TearDown()
		{
			OnTearDown();
			DropDatabaseSchema();
			container.Dispose();
			container = null;
		}

		protected virtual void ConfigureContainer()
		{
		}


		public virtual void OnSetUp()
		{
		}

		public virtual void OnTearDown()
		{
		}


		protected string GetContainerFile()
		{
			return "Castle.Facilities.NHibernateIntegration.Tests/" + ConfigurationFile;
		}
	}
}