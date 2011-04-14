using System;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction.NHibernate;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using NHibernate;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.vNext
{
	internal class NHFacilityInitTests
	{
		private class C1 : INHibernateInstaller
		{
			public bool IsDefault { get { return true; } }
			public string SessionFactoryKey { get { return "C1"; } }
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
			public bool IsDefault { get { return false; } }
			public string SessionFactoryKey { get { return "C2"; } }
			public FluentConfiguration BuildFluent()
			{
				return new ExampleInstaller().BuildFluent();
			}

			public void Registered(ISessionFactory factory)
			{
				throw new ApplicationException("C2");
			}
		}

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
			var c = new WindsorContainer();
			try
			{
				c.AddFacility<NHFacility>();
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
			var c = new WindsorContainer();
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<C2>());
			try
			{
				c.AddFacility<NHFacility>();
				Assert.Fail();
			}
			catch (FacilityException ex)
			{
				Assert.That(ex.Message, Is.StringContaining("IsDefault"));
			}
		}

		private void AssertOrder(WindsorContainer c)
		{
			try
			{
				c.AddFacility<NHFacility>();
				NUnit.Framework.Assert.Fail("no exception thrown");
			}
			catch (ApplicationException ex)
			{
				NUnit.Framework.Assert.That(ex.Message, Is.EqualTo("C1"));
			}
		}
	}
}