#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction.NHibernate;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	internal class ExampleInstaller : INHibernateInstaller
	{
		public bool IsDefault
		{
			get { return true; }
		}

		public string SessionFactoryKey
		{
			get { return "sf.default"; }
		}

		public FluentConfiguration BuildFluent()
		{
			return Fluently.Configure()
				.Database(
					MsSqlConfiguration.MsSql2008
						.DefaultSchema("dbo")
						.ConnectionString(ConfigurationManager.ConnectionStrings["test"].ConnectionString)
				)
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<ThingMap>());
		}

		public void Registered(ISessionFactory factory)
		{
		}
	}

	public class ThingMap : ClassMap<Thing>
	{
		public ThingMap()
		{
			Not.LazyLoad();
			Id(x => x.ID).GeneratedBy.GuidComb();
			Map(x => x.Value).Column("val");
		}
	}

	public class Thing
	{
		protected Thing()
		{
		}

		public Thing(double val)
		{
			Value = val;
		}

		public Guid ID { get; protected set; }
		public double Value { get; protected set; }
	}

	public class SimpleNHibernateIntegration
	{
		[Test]
		public void Saving()
		{
			// in your app_start:
			var c = new WindsorContainer();
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());
			c.AddFacility<FactorySupportFacility>();
			c.AddFacility<NHFacility>();

			c.Register(Component.For<SimpleNHibernateUsingClass>().LifeStyle.HybridPerWebRequestTransient());

			// your controller calls:
			var service = c.Resolve<SimpleNHibernateUsingClass>();
			service.SaveThing();
			Assert.That(service.VerifyThing(), Is.Not.Null);
		}
	}

	internal class SimpleNHibernateUsingClass
	{
		private readonly ISession _Session;
		private readonly IStatelessSession _StatelessSession;
		private Guid _ThingId;

		public SimpleNHibernateUsingClass(ISession session, IStatelessSession statelessSession)
		{
			Contract.Requires(session != null);
			_Session = session;
			_StatelessSession = statelessSession;
		}

		[Transaction]
		public virtual void SaveThing()
		{
			_ThingId = (Guid)_Session.Save(new Thing(4.6));
		}

		[Transaction]
		public virtual Thing VerifyThing()
		{
			Assert.That(_StatelessSession.Get<Thing>(_ThingId), Is.Not.Null);
			Assert.That(_StatelessSession.Transaction, Is.Not.Null);
			Assert.That(_StatelessSession.Transaction.IsActive);
			Assert.That(_Session.Transaction, Is.Not.Null);
			Assert.That(_Session.Transaction.IsActive);

			// for testing we need to make sure it's not just in the FLC.
			_Session.Clear();
			return _Session.Load<Thing>(_ThingId);
		}
	}
}