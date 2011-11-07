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

namespace Castle.Facilities.NHibernateIntegration
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using Builders;
	using Core.Configuration;
	using Core.Logging;
	using Internal;
	using MicroKernel;
	using MicroKernel.Facilities;
	using MicroKernel.Registration;
	using MicroKernel.SubSystems.Conversion;
	using NHibernate;
	using Services.Transaction;
	using SessionStores;
	using IInterceptor = NHibernate.IInterceptor;
	using ILogger = Core.Logging.ILogger;
	using ILoggerFactory = Core.Logging.ILoggerFactory;

	/// <summary>
	/// Provides a basic level of integration with the NHibernate project
	/// </summary>
	/// <remarks>
	/// This facility allows components to gain access to the NHibernate's 
	/// objects:
	/// <list type="bullet">
	///   <item><description>NHibernate.Cfg.Configuration</description></item>
	///   <item><description>NHibernate.ISessionFactory</description></item>
	/// </list>
	/// <para>
	/// It also allow you to obtain the ISession instance 
	/// through the component <see cref="ISessionManager"/>, which is 
	/// transaction aware and save you the burden of sharing session
	/// or using a singleton.
	/// </para>
	/// </remarks>
	/// <example>The following sample illustrates how a component 
	/// can access the session.
	/// <code>
	/// public class MyDao
	/// {
	///   private ISessionManager sessionManager;
	/// 
	///   public MyDao(ISessionManager sessionManager)
	///   {
	///     this.sessionManager = sessionManager;
	///   } 
	///   
	///   public void Save(Data data)
	///   {
	///     using(ISession session = sessionManager.OpenSession())
	///     {
	///       session.Save(data);
	///     }
	///   }
	/// }
	/// </code>
	/// </example>
	public class NHibernateFacility : AbstractFacility
	{
		private const string DefaultConfigurationBuilderKey = "nhfacility.configuration.builder";
	    internal const string ConfigurationBuilderConfigurationKey = "configurationBuilder";
		private const string SessionFactoryResolverKey = "nhfacility.sessionfactory.resolver";
		private const string SessionInterceptorKey = "nhibernate.sessionfactory.interceptor";
		internal const string IsWebConfigurationKey = "isWeb";
	    internal const string CustomStoreConfigurationKey = "customStore";
	    internal const string DefaultFlushModeConfigurationKey = "defaultFlushMode";
		private const string SessionManagerKey = "nhfacility.sessionmanager";
		private const string TransactionManagerKey = "nhibernate.transaction.manager";
	    internal const string SessionFactoryIdConfigurationKey = "id";
	    internal const string SessionFactoryAliasConfigurationKey = "alias";
		private const string SessionStoreKey = "nhfacility.sessionstore";
		private const string ConfigurationBuilderForFactoryFormat = "{0}.configurationBuilder";


		private readonly IConfigurationBuilder configurationBuilder;

		private ILogger log = NullLogger.Instance;
	    private Type customConfigurationBuilderType;
	    private NHibernateFacilityConfiguration facilitySettingConfig;

	    /// <summary>
		/// Instantiates the facility with the specified configuration builder.
		/// </summary>
		/// <param name="configurationBuilder"></param>
		public NHibernateFacility(IConfigurationBuilder configurationBuilder)
		{
			this.configurationBuilder = configurationBuilder;
            facilitySettingConfig = new NHibernateFacilityConfiguration(configurationBuilder);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateFacility"/> class.
		/// </summary>
		public NHibernateFacility() : this(new DefaultConfigurationBuilder())
		{
		}

		/// <summary>
		/// The custom initialization for the Facility.
		/// </summary>
		/// <remarks>It must be overriden.</remarks>
		protected override void Init()
		{
			if (Kernel.HasComponent(typeof (ILoggerFactory)))
			{
				log = Kernel.Resolve<ILoggerFactory>().Create(GetType());
			}

		    facilitySettingConfig.Init(FacilityConfig, Kernel);

			AssertHasConfig();
			AssertHasAtLeastOneFactoryConfigured();
			RegisterComponents();
			ConfigureFacility();
		}

		#region Set up of components

		/// <summary>
		/// Registers the session factory resolver, the session store, the session manager and the transaction manager.
		/// </summary>
		protected virtual void RegisterComponents()
		{
			Kernel.Register(Component.For<NHSessionInterceptor>().Named("nhsession.interceptor"));
			Kernel.ComponentModelBuilder.AddContributor(new NHSessionComponentInspector());

			RegisterDefaultConfigurationBuilder();
			RegisterSessionFactoryResolver();
			RegisterSessionStore();
			RegisterSessionManager();
			RegisterTransactionManager();
		}


		/// <summary>
		/// Register <see cref="IConfigurationBuilder"/> the default ConfigurationBuilder or (if present) the one 
		/// specified via "configurationBuilder" attribute.
		/// </summary>
		private void RegisterDefaultConfigurationBuilder()
		{
		    if (!facilitySettingConfig.HasConcreteConfigurationBuilder())
            {
                customConfigurationBuilderType = facilitySettingConfig.GetConfigurationBuilderType();

                if (facilitySettingConfig.HasConfigurationBuilderType())
				{
					if (!typeof(IConfigurationBuilder).IsAssignableFrom(customConfigurationBuilderType))
					{
						throw new FacilityException(
							string.Format(
								"ConfigurationBuilder type '{0}' invalid. The type must implement the IConfigurationBuilder contract",
								customConfigurationBuilderType.FullName));
					}
				}

			    Kernel.Register(Component
				                	.For<IConfigurationBuilder>()
                                    .ImplementedBy(customConfigurationBuilderType)
				                	.Named(DefaultConfigurationBuilderKey));
			}
			else
				Kernel.Register(
					Component.For<IConfigurationBuilder>().Instance(configurationBuilder).Named(DefaultConfigurationBuilderKey));
		}


	    /// <summary>
		/// Registers <see cref="SessionFactoryResolver"/> as the session factory resolver.
		/// </summary>
		protected void RegisterSessionFactoryResolver()
		{
			Kernel.Register(
				Component.For<ISessionFactoryResolver>()
					.ImplementedBy<SessionFactoryResolver>()
					.Named(SessionFactoryResolverKey).LifeStyle.Singleton
				);
		}

		/// <summary>
		/// Registers the configured session store.
		/// </summary>
		protected void RegisterSessionStore()
		{
			Kernel.Register(Component.For<ISessionStore>().ImplementedBy(facilitySettingConfig.GetSessionStoreType()).Named(SessionStoreKey));
		}

		/// <summary>
		/// Registers <see cref="DefaultSessionManager"/> as the session manager.
		/// </summary>
		protected void RegisterSessionManager()
		{
		    string defaultFlushMode = facilitySettingConfig.FlushMode;

			if (!string.IsNullOrEmpty(defaultFlushMode))
			{
				MutableConfiguration confignode = new MutableConfiguration(SessionManagerKey);

				IConfiguration properties = new MutableConfiguration("parameters");
				confignode.Children.Add(properties);

				properties.Children.Add(new MutableConfiguration("DefaultFlushMode", defaultFlushMode));

				Kernel.ConfigurationStore.AddComponentConfiguration(SessionManagerKey, confignode);
			}

			Kernel.Register(Component.For<ISessionManager>().ImplementedBy<DefaultSessionManager>().Named(SessionManagerKey));
		}

		/// <summary>
		/// Registers <see cref="DefaultTransactionManager"/> as the transaction manager.
		/// </summary>
		protected void RegisterTransactionManager()
		{
			if (!Kernel.HasComponent(typeof (ITransactionManager)))
			{
				log.Info("No Transaction Manager registered on Kernel, registering default Transaction Manager");
				Kernel.Register(
					Component.For<ITransactionManager>().ImplementedBy<DefaultTransactionManager>().Named(TransactionManagerKey));
			}
		}

		#endregion

		#region Configuration methods

		/// <summary>
		/// Configures the facility.
		/// </summary>
        protected void ConfigureFacility()
		{
		    ISessionFactoryResolver sessionFactoryResolver = Kernel.Resolve<ISessionFactoryResolver>();

		    ConfigureReflectionOptimizer();

		    bool firstFactory = true;

            foreach (var factoryConfig in facilitySettingConfig.Factories)
		    {
		        ConfigureFactories(factoryConfig, sessionFactoryResolver, firstFactory);
		        firstFactory = false;
		    }
		}

	    /// <summary>
	    /// Reads the attribute <c>useReflectionOptimizer</c> and configure
	    /// the reflection optimizer accordingly.
	    /// </summary>
	    /// <remarks>
	    /// As reported on Jira (FACILITIES-39) the reflection optimizer
	    /// slow things down. So by default it will be disabled. You
	    /// can use the attribute <c>useReflectionOptimizer</c> to turn it
	    /// on. 
	    /// </remarks>
	    private void ConfigureReflectionOptimizer()
		{
	        NHibernate.Cfg.Environment.UseReflectionOptimizer = facilitySettingConfig.ShouldUseReflectionOptimizer();
		}

		/// <summary>
		/// Configures the factories.
		/// </summary>
		/// <param name="config">The config.</param>
		/// <param name="sessionFactoryResolver">The session factory resolver.</param>
		/// <param name="firstFactory">if set to <c>true</c> [first factory].</param>
		protected void ConfigureFactories(NHibernateFactoryConfiguration config, ISessionFactoryResolver sessionFactoryResolver, bool firstFactory)
		{
			String id = config.Id;

			if (string.IsNullOrEmpty(id))
			{
				String message = "You must provide a " +
				                 "valid 'id' attribute for the 'factory' node. This id is used as key for " +
				                 "the ISessionFactory component registered on the container";

				throw new ConfigurationErrorsException(message);
			}

            String alias = config.Alias;

			if (!firstFactory && (string.IsNullOrEmpty(alias)))
			{
				String message = "You must provide a " +
				                 "valid 'alias' attribute for the 'factory' node. This id is used to obtain " +
				                 "the ISession implementation from the SessionManager";

				throw new ConfigurationErrorsException(message);
			}

			if (string.IsNullOrEmpty(alias))
			{
				alias = Constants.DefaultAlias;
			}

            string configurationBuilderType = config.ConfigurationBuilderType;
			string configurationbuilderKey = string.Format(ConfigurationBuilderForFactoryFormat, id);
			IConfigurationBuilder configBuilder;
			if (string.IsNullOrEmpty(configurationBuilderType))
			{
				configBuilder = Kernel.Resolve<IConfigurationBuilder>();
			}
			else
			{
				Kernel.Register(
					Component.For<IConfigurationBuilder>().ImplementedBy(Type.GetType(configurationBuilderType)).Named(
						configurationbuilderKey));
				configBuilder = Kernel.Resolve<IConfigurationBuilder>(configurationbuilderKey);
			}

            var cfg = configBuilder.GetConfiguration(config.GetConfiguration());

			// Registers the Configuration object
			Kernel.Register(Component.For<NHibernate.Cfg.Configuration>().Instance(cfg).Named(String.Format("{0}.cfg", id)));

			// If a Session Factory level interceptor was provided, we use it
			if (Kernel.HasComponent(SessionInterceptorKey))
			{
				cfg.Interceptor = Kernel.Resolve<IInterceptor>(SessionInterceptorKey);
			}

			// Registers the ISessionFactory as a component
			Kernel.Register(Component
			                	.For<ISessionFactory>()
			                	.Named(id)
			                	.Activator<SessionFactoryActivator>()
			                	.ExtendedProperties(Property.ForKey(Constants.SessionFactoryConfiguration).Eq(cfg))
			                	.LifeStyle.Singleton);

			sessionFactoryResolver.RegisterAliasComponentIdMapping(alias, id);
		}

		#endregion

		#region Helper methods

		private void AssertHasAtLeastOneFactoryConfigured()
		{
            if (facilitySettingConfig.HasValidFactory()) return;

			IConfiguration factoriesConfig = FacilityConfig.Children["factory"];

			if (factoriesConfig == null)
			{
				String message = "You need to configure at least one factory to use the NHibernateFacility";

				throw new ConfigurationErrorsException(message);
			}
		}

		private void AssertHasConfig()
		{
			if (!facilitySettingConfig.IsValid())
			{
				String message = "The NHibernateFacility requires configuration";

				throw new ConfigurationErrorsException(message);
			}
		}

		#endregion

        #region FluentConfiguration

        /// <summary>
        /// Sets a custom <see cref="IConfigurationBuilder"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public NHibernateFacility ConfigurationBuilder<T>()
            where T : IConfigurationBuilder
        {            
            return ConfigurationBuilder(typeof (T));
        }

        /// <summary>
        /// Sets a custom <see cref="IConfigurationBuilder"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
	    public NHibernateFacility ConfigurationBuilder(Type type)
        {
            facilitySettingConfig.ConfigurationBuilder(type);
	        return this;
        }

        /// <summary>
        /// Sets the facility to work on a web conext
        /// </summary>
        /// <returns></returns>
        public NHibernateFacility IsWeb()
        {
            facilitySettingConfig.IsWeb();
            return this;
        }
        #endregion
	}

    internal class NHibernateFacilityConfiguration 
    {
        private IConfigurationBuilder configurationBuilderInstance;
        private Type configurationBuilderType;
        private IConfiguration facilityConfig;
        private bool isWeb;
        private bool useReflectionOptimizer = false;
        private IKernel kernel;
        private Type customStore;


        public IEnumerable<NHibernateFactoryConfiguration> Factories { get; set; }

        ///<summary>
        ///</summary>
        ///<param name="configurationBuilderInstance"></param>
        public NHibernateFacilityConfiguration(IConfigurationBuilder configurationBuilderInstance)
        {
            this.configurationBuilderInstance = configurationBuilderInstance;
            Factories = Enumerable.Empty<NHibernateFactoryConfiguration>();
        }

        public bool OnWeb
        {
            get { return isWeb; }
        }

        public string FlushMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="ker"></param>
        public void Init(IConfiguration con, IKernel ker)
        {
            facilityConfig = con;
            kernel = ker;

            if (ConfigurationIsValid())
            {
                ConfigureWithExternalConfiguration();
            }
            else
            {
                Factories = new[]
                                {
                                    new NHibernateFactoryConfiguration(new MutableConfiguration("factory"))
                                        {
                                            Id = "factory_1"
                                        }
                                };
            }
        }

        private void ConfigureWithExternalConfiguration()
        {
            var customConfigurationBuilder = facilityConfig.Attributes[NHibernateFacility.ConfigurationBuilderConfigurationKey];

            if (!string.IsNullOrEmpty(customConfigurationBuilder))
            {

                var converter = (IConversionManager) kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);

                try
                {
                    ConfigurationBuilder(
                        (Type) converter.PerformConversion(customConfigurationBuilder, typeof (Type)));
                }
                catch (ConverterException)
                {
                    throw new FacilityException(string.Format(
                        "ConfigurationBuilder type '{0}' invalid or not found", customConfigurationBuilder));
                }
            }

            BuildFactories();


            if (facilityConfig.Attributes[NHibernateFacility.CustomStoreConfigurationKey] != null)
            {
                var customStoreType = facilityConfig.Attributes[NHibernateFacility.CustomStoreConfigurationKey];
                var converter = (ITypeConverter)kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
                SessionStore((Type)converter.PerformConversion(customStoreType, typeof(Type)));
            }

            FlushMode = facilityConfig.Attributes[NHibernateFacility.DefaultFlushModeConfigurationKey];

            bool.TryParse(facilityConfig.Attributes[NHibernateFacility.IsWebConfigurationKey], out isWeb);
        }

        private bool ConfigurationIsValid()
        {
            return facilityConfig != null && facilityConfig.Children.Count > 0;
        }

        private void BuildFactories()
        {
            Factories = facilityConfig
                .Children
                .Select(config => new NHibernateFactoryConfiguration(config));
        }

        public void ConfigurationBuilder(Type type)
        {
            configurationBuilderInstance = null;
            configurationBuilderType = type;
        }

        public void SessionStore(Type type)
        {
            if (!typeof(ISessionStore).IsAssignableFrom(type))
            {
                var message = "The specified customStore does " +
                                 "not implement the interface ISessionStore. Type " + type;
                throw new ConfigurationErrorsException(message);
            }

            customStore = type;
        }

        public void ConfigurationBuilder(IConfigurationBuilder builder)
        {
            configurationBuilderInstance = builder;
        }

        public void IsWeb()
        {
            isWeb = true;
        }

        public bool IsValid()
        {
            return facilityConfig != null || (configurationBuilderInstance != null || configurationBuilderType != null);
        }

        public bool HasValidFactory()
        {
            return Factories.Count() > 0;
        }

        public bool ShouldUseReflectionOptimizer()
        {
            if (facilityConfig != null)
            {
                bool result;
                if (bool.TryParse(facilityConfig.Attributes["useReflectionOptimizer"], out result))
                    return result;

                return false;
            }

            return useReflectionOptimizer;
        }

        public bool HasConcreteConfigurationBuilder()
        {
            return configurationBuilderInstance != null && !HasConfigurationBuilderType();
        }

        public Type GetConfigurationBuilderType()
        {
            return configurationBuilderType;
        }

        public bool HasConfigurationBuilderType()
        {
            return configurationBuilderType != null;
        }

        public Type GetSessionStoreType()
        {
			// Default implementation
			Type sessionStoreType = typeof (CallContextSessionStore);

            if (isWeb)
				sessionStoreType = typeof (WebSessionStore);

			if (customStore != null)
			{
			    sessionStoreType = customStore;
			}

            return sessionStoreType;
        }
    }

    ///<summary>
    ///</summary>
	public class NHibernateFactoryConfiguration
	{
        private IConfiguration config;

        ///<summary>
        ///</summary>
        ///<param name="config"></param>
        ///<exception cref="ArgumentNullException"></exception>
        public NHibernateFactoryConfiguration(IConfiguration config)
        {
            if ( config == null) throw new ArgumentNullException("config");

            this.config = config;

            Id = config.Attributes[NHibernateFacility.SessionFactoryIdConfigurationKey];
            Alias = config.Attributes[NHibernateFacility.SessionFactoryAliasConfigurationKey];
            ConfigurationBuilderType = config.Attributes[NHibernateFacility.ConfigurationBuilderConfigurationKey];
        }

        /// <summary>
        /// Get or sets the factory Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the factory Alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the factory ConfigurationBuilder
        /// </summary>
        public string ConfigurationBuilderType { get; set; }

        /// <summary>
        /// Constructs an IConfiguration instance for this factory
        /// </summary>
        /// <returns></returns>
        public IConfiguration GetConfiguration()
        {
            return config;
        }


    }
}