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
using System.Linq;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using NHibernate;
using NHibernate.Cfg;

namespace Castle.Services.vNextTransaction.NHibernate
{
	public class NHFacility : AbstractFacility
	{
		protected override void Init()
		{
			var installer = Kernel.ResolveAll<INHibernateInstaller>();

			if (installer.Length == 0)
				throw new FacilityException("no INHibernateInstaller-s registered.");

			var count = installer.Count(x => x.IsDefault);
			if (count == 0 || count > 1)
				throw new FacilityException("no INHibernateInstaller has IsDefault = true or many have specified it");

			if (!installer.All(x => !string.IsNullOrEmpty(x.SessionFactoryKey)))
				throw new FacilityException("all session factory keys must be non null and non empty strings");

			if (!Kernel.HasComponent(typeof(FactorySupportFacility)))
				throw new FacilityException("you need factory support facility to run NHFacility");
				
			var installed = installer
				.Select(x => new {
					Config = x.BuildFluent().BuildConfiguration(),
					Instance = x
				})
				.Select(x =>new { x.Config, x.Instance, Factory = x.Config.BuildSessionFactory() })
				.OrderByDescending(x => x.Instance.IsDefault)
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
						.LifeStyle.HybridPerWebRequestTransient()
						.Named(x.Instance.SessionFactoryKey + "-session")
						.UsingFactoryMethod((k, cc) =>
							{
								var s = k.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey)
									.OpenSession();
								s.FlushMode = FlushMode.Commit;
								return s;
							}),
					Component.For<IStatelessSession>()
						.LifeStyle.HybridPerWebRequestTransient()
						.Named(x.Instance.SessionFactoryKey + "-s-session")
						.UsingFactoryMethod(k => k.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey)
						                         	.OpenStatelessSession())
						
						
						))
				.ToList();

			// notify the installers
			installed.Run(x => x.Instance.Registered(x.Factory));
		}
	}
}