using System.Configuration;
using Castle.Facilities.NHibernate;
using Castle.Services.vNextTransaction;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace Castle.Services.Transaction.Tests.vNext
{
	internal class ExampleInstaller : INHibernateInstaller
	{
		private readonly Maybe<IInterceptor> _Interceptor;

		public ExampleInstaller()
		{
			_Interceptor = Maybe.None<IInterceptor>();
		}

		public ExampleInstaller(IInterceptor interceptor)
		{
			_Interceptor = Maybe.Some(interceptor);
		}

		public Maybe<IInterceptor> Interceptor
		{
			get { return _Interceptor; }
		}

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
				.Database(MsSqlConfiguration.MsSql2008.DefaultSchema("dbo")
							.ConnectionString(ConfigurationManager.ConnectionStrings["test"].ConnectionString))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<ThingMap>());
		}

		public void Registered(ISessionFactory factory)
		{
		}
	}
}