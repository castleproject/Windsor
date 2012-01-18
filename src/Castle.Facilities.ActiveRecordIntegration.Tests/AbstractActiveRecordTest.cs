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

namespace Castle.Facilities.ActiveRecordIntegration.Tests
{
	using System;
	using Castle.Core.Resource;
	using Castle.Facilities.AutoTx;
	using Castle.Windsor;
	
	using Castle.ActiveRecord;
	using Castle.Windsor.Configuration.Interpreters;
	using MicroKernel.Registration;
	using NUnit.Framework;

	using Castle.Facilities.ActiveRecordIntegration.Tests.Model;

	public class AbstractActiveRecordTest
	{
		protected IWindsorContainer container;

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer(new XmlInterpreter(new ConfigResource()));

			container.AddFacility("transactionfacility", new TransactionFacility() );
			container.AddFacility("arfacility", new ActiveRecordFacility() );

			container.Register(Component.For<BlogService>().Named("blog.service"));
			container.Register(Component.For<PostService>().Named("post.service"));
			container.Register(Component.For<FirstService>().Named("first.service"));
			container.Register(Component.For<WiringSession>().Named("wiring.service"));

			Recreate();
		}

		[TearDown]
		public void Terminate()
		{
			try
			{
				ActiveRecordStarter.DropSchema();
			}
			catch(Exception)
			{
				
			}

			container.Dispose();
		}

		protected void Recreate()
		{
			ActiveRecordStarter.CreateSchema();
		}
	}
}
