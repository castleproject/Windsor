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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Facilities.FactorySupport;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.Services.vNextTransaction;
using log4net;
using NHibernate;
using NHibernate.Cfg;

namespace Castle.Facilities.NHibernate
{
	public class NHibernateFacility : AbstractFacility
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (NHibernateFacility));
		
		[ContractVerification(false)] // interactive bits don't have contracts
		protected override void Init()
		{
			_Logger.DebugFormat("initializing NHibernateFacility");

			var installers = Kernel.ResolveAll<INHibernateInstaller>();

			Contract.Assume(installers != null, "ResolveAll shouldn't return null");

			if (installers.Length == 0)
				throw new FacilityException("no INHibernateInstaller-s registered.");

			var count = installers.Count(x => x.IsDefault);
			if (count == 0 || count > 1)
				throw new FacilityException("no INHibernateInstaller has IsDefault = true or many have specified it");

			if (!installers.All(x => !string.IsNullOrEmpty(x.SessionFactoryKey)))
				throw new FacilityException("all session factory keys must be non null and non empty strings");

			VerifyLegacyInterceptors();

			AddFacility<AutoTxFacility>();
			AddFacility<FactorySupportFacility>();
			AddFacility<TypedFactoryFacility>();

			_Logger.DebugFormat("registering facility components");

			var added = new HashSet<string>();

			var installed = installers
				.Select(x => new {
					Config = x.BuildFluent().BuildConfiguration(),
					Instance = x
				})
				.Select(x =>new { x.Config, x.Instance, Factory = x.Config.BuildSessionFactory() })
				.OrderByDescending(x => x.Instance.IsDefault)
				.Do(x => {
					if (!added.Add(x.Instance.SessionFactoryKey))
						throw new FacilityException(string.Format("Duplicate session factory keys '{0}' added. Verify that your INHibernateInstaller instances are not named the same.", x.Instance.SessionFactoryKey));
				})
				.Do(x => Kernel.Register(
					Component.For<Configuration>()
						.Instance(x.Config)
						.LifeStyle.Singleton
						.Named(x.Instance.SessionFactoryKey + "-cfg"),
					Component.For<ISessionFactory>()
						.Instance(x.Factory)
						.LifeStyle.Singleton
						.Named(x.Instance.SessionFactoryKey),
					Component.For<ISession>()
						.LifeStyle.HybridPerTransactionTransient()
						.Named(x.Instance.SessionFactoryKey + "-session")
						.UsingFactoryMethod(k => {
						    var factory = k.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey);
						    var s = x.Instance.Interceptor.Do(y => factory.OpenSession(y)).OrDefault(factory.OpenSession());
							s.FlushMode = FlushMode.Commit;
							return s;
						}),
					Component.For<ISessionManager>().Instance(new SessionManager(() => {
							var factory = Kernel.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey);
							var s = x.Instance.Interceptor.Do(y => factory.OpenSession(y)).OrDefault(factory.OpenSession());
							s.FlushMode = FlushMode.Commit;
							return s;
						}))
						.Named(x.Instance.SessionFactoryKey + "-manager")
						.LifeStyle.Singleton,
					Component.For<IStatelessSession>()
						.LifeStyle.HybridPerTransactionTransient()
						.Named(x.Instance.SessionFactoryKey + "-s-session")
						.UsingFactoryMethod(k => 
							k.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey).OpenStatelessSession())
					))
				.ToList();

			_Logger.Debug("notifying the nhibernate installers that they have been configured");

			installed.Run(x => x.Instance.Registered(x.Factory));

			_Logger.Debug("initialized NHibernateFacility");
		}

		private void VerifyLegacyInterceptors()
		{
			if (Kernel.HasComponent("nhibernate.session.interceptor"))
				_Logger.Warn("component with key \"nhibernate.session.interceptor\" found! this interceptor will not be used.");
		}

		// even though this is O(3n), n ~= 3, so we don't mind it
		private void AddFacility<T>() where T : IFacility, new()
		{
			var facilities = Kernel.GetFacilities();

			Contract.Assume(facilities != null, "GetFacilities shouldn't return null");

			if (!facilities.Select(x => x.ToString()).Contains(typeof(T).ToString()))
			{
				_Logger.InfoFormat("facility '{0}' wasn't found in kernel, adding it, because it's a requirement for NHibernateFacility",
					typeof(T));

				Kernel.AddFacility<T>();
			}
		}
	}
}