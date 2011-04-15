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
using Castle.Facilities.FactorySupport;
using Castle.Facilities.NHibernate;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using NHibernate;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	internal class NHibernateFacility_Init
	{
		[Test]
		public void given_two_configs_resolves_the_default_true_one_first()
		{
			var c = new WindsorContainer();
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C1>());
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C2>());
			AssertOrder(c);
		}

		[Test]
		public void given_two_configs_resolves_the_default_true_one_first_permutate()
		{
			var c = new WindsorContainer();
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C2>());
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C1>());
			AssertOrder(c);
		}

		[Test]
		public void facility_exception_cases()
		{
			var c = GetTxContainer();
			try
			{
				c.AddFacility<NHibernateFacility>();
				Assert.Fail();
			}
			catch (FacilityException ex)
			{
				Assert.That(ex.Message, Is.StringContaining("registered"));
			}
		}

		[Test]
		public void facility_exception_cases_no_default()
		{
			var c = GetTxContainer();

			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C2>());
			try
			{
				c.AddFacility<NHibernateFacility>();
				Assert.Fail();
			}
			catch (FacilityException ex)
			{
				Assert.That(ex.Message, Is.StringContaining("IsDefault"));
			}
		}

		[Test]
		public void facility_exception_duplicate_keys()
		{
			var c = GetTxContainer();

			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C1>());
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C1_Copy>());
			try
			{
				c.AddFacility<NHibernateFacility>();
				Assert.Fail();
			}
			catch (FacilityException ex)
			{
				Assert.That(ex.Message.ToLowerInvariant(), Is.StringContaining("duplicate"));
			}
		}

		private static WindsorContainer GetTxContainer()
		{
			var c = new WindsorContainer();
			c.AddFacility<FactorySupportFacility>();
			c.AddFacility<AutoTxFacility>();
			return c;
		}

		private void AssertOrder(WindsorContainer c)
		{
			c.AddFacility<FactorySupportFacility>();
			c.AddFacility<AutoTxFacility>();

			try
			{
				c.AddFacility<NHibernateFacility>();
				Assert.Fail("no exception thrown");
			}
			catch (ApplicationException ex)
			{
				Assert.That(ex.Message, Is.EqualTo("C1"));
			}
		}

		#region Installers

		private class C1 : INHibernateInstaller
		{
			public bool IsDefault
			{
				get { return true; }
			}

			public string SessionFactoryKey
			{
				get { return "C1"; }
			}

			public Maybe<IInterceptor> Interceptor
			{
				get { return Maybe.None<IInterceptor>(); }
			}

			public FluentConfiguration BuildFluent()
			{
				return new ExampleInstaller().BuildFluent();
			}

			public void Registered(ISessionFactory factory)
			{
				throw new ApplicationException("C1");
			}
		}

		private class C2 : INHibernateInstaller
		{
			public bool IsDefault
			{
				get { return false; }
			}

			public string SessionFactoryKey
			{
				get { return "C2"; }
			}

			public Maybe<IInterceptor> Interceptor
			{
				get { return Maybe.None<IInterceptor>(); }
			}

			public FluentConfiguration BuildFluent()
			{
				return new ExampleInstaller().BuildFluent();
			}

			public void Registered(ISessionFactory factory)
			{
				throw new ApplicationException("C2");
			}
		}


		private class C1_Copy : INHibernateInstaller
		{
			public bool IsDefault
			{
				get { return false; }
			}

			public string SessionFactoryKey
			{
				get { return "C1"; }
			}

			public Maybe<IInterceptor> Interceptor
			{
				get { return Maybe.None<IInterceptor>(); }
			}

			public FluentConfiguration BuildFluent()
			{
				return new ExampleInstaller().BuildFluent();
			}

			public void Registered(ISessionFactory factory)
			{
				throw new ApplicationException("C1");
			}
		}

		#endregion
	}
}