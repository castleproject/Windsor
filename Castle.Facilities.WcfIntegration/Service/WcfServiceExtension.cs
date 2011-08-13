
namespace Castle.Facilities.WcfIntegration
{
	using System;
#if DOTNET40
	using System.Collections.Concurrent;
#endif
	using System.Collections.Generic;
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Channels;
	using Castle.Core;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.Facilities.WcfIntegration.Rest;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public class WcfServiceExtension : IDisposable
	{
		private IKernel kernel;
		private WcfFacility facility;
		private Binding defaultBinding;
		private TimeSpan? closeTimeout;
		private Action afterInit;
		private readonly List<ServiceHost> hosts = new List<ServiceHost>();
		private readonly static object sync = new object();

		internal static IKernel GlobalKernel;

		#region ServiceHostBuilder Delegate Fields 
	
		private delegate ServiceHost CreateServiceHostDelegate(
			IKernel Kernel, IWcfServiceModel serviceModel, ComponentModel model,
			Uri[] baseAddresses);

		private static readonly MethodInfo createServiceHostMethod =
			typeof(WcfServiceExtension).GetMethod("CreateServiceHostInternal",
				BindingFlags.NonPublic | BindingFlags.Static, null,
				new Type[] { typeof(IKernel), typeof(IWcfServiceModel),
					typeof(ComponentModel), typeof(Uri[]) }, null
				);

		private static readonly ConcurrentDictionary<Type, CreateServiceHostDelegate>
			createServiceHostCache = new ConcurrentDictionary<Type, CreateServiceHostDelegate>();

		#endregion

		public Binding DefaultBinding
		{
			get { return defaultBinding ?? facility.DefaultBinding; }
			set { defaultBinding = value; }
		}

		public TimeSpan? CloseTimeout
		{
			get { return closeTimeout ?? facility.CloseTimeout; }
			set { closeTimeout = value; }
		}

		public bool OpenServiceHostsEagerly { get; set; }

		public AspNetCompatibilityRequirementsMode? AspNetCompatibility { get; set; }

		public IEnumerable<ServiceHost> ManagedServiceHosts
		{
			get { return hosts; }
		}

		internal void Init(IKernel kernel, WcfFacility facility)
		{
			this.kernel = kernel;
			this.facility = facility;
			GlobalKernel = kernel;

			ConfigureAspNetCompatibility();
			AddDefaultServiceHostBuilders();

			kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
			kernel.ComponentRegistered += Kernel_ComponentRegistered;

			if (afterInit != null)
			{
				afterInit();
				afterInit = null;
			}
		}

		public void Dispose()
		{
			ReleaseServiceHosts();
		}

		public WcfServiceExtension AddServiceHostBuilder<TBuilder>()
			where TBuilder : IServiceHostBuilder
		{
			return AddServiceHostBuilder(typeof(TBuilder));
		}

		public WcfServiceExtension AddServiceHostBuilder(Type builder)
		{
			AddServiceHostBuilder(builder, true);
			return this;
		}

		private void Kernel_ComponentModelCreated(ComponentModel model)
		{
			ExtensionDependencies dependencies = null;

			foreach (var serviceModel in ResolveServiceModels(model))
			{
				if (dependencies == null)
				{
					dependencies = new ExtensionDependencies(model, kernel)
						.Apply(new WcfServiceExtensions())
						.Apply(new WcfEndpointExtensions(WcfExtensionScope.Services));
				}

				if (serviceModel != null)
				{
					dependencies.Apply(serviceModel.Extensions);
					
					foreach (var endpoint in serviceModel.Endpoints)
					{
						dependencies.Apply(endpoint.Extensions);
					}
				}
			}
		}

		private void Kernel_ComponentRegistered(string key, IHandler handler)
		{
			var model = handler.ComponentModel;

			foreach (var serviceModel in ResolveServiceModels(model))
			{ 
				if (serviceModel.IsHosted == false)
				{
					CreateServiceHostWhenHandlerIsValid(handler, serviceModel, model);
				}
			}
		}

		private void ConfigureAspNetCompatibility()
		{
			if (AspNetCompatibility.HasValue)
			{
				kernel.Register(
					Component.For<AspNetCompatibilityRequirementsAttribute>()
						.Instance(new AspNetCompatibilityRequirementsAttribute
						{
							RequirementsMode = AspNetCompatibility.Value
						})
					);
			}
		}

		private void AddDefaultServiceHostBuilders()
		{
			AddServiceHostBuilder(typeof(DefaultServiceHostBuilder), false);
			AddServiceHostBuilder(typeof(RestServiceHostBuilder), false);
		}

		internal void AddServiceHostBuilder(Type builder, bool force)
		{
			if (typeof(IServiceHostBuilder).IsAssignableFrom(builder) == false)
			{
				throw new ArgumentException(string.Format(
					"The type {0} does not represent an IServiceHostBuilder.",
					builder.FullName), "builder");
			}

			var serviceHostBuilder = WcfUtils.GetClosedGenericDefinition(typeof(IServiceHostBuilder<>), builder);

			if (serviceHostBuilder == null)
			{
				throw new ArgumentException(string.Format(
					"The service model cannot be inferred from the builder {0}.  Did you implement IServiceHostBuilder<>?",
					builder.FullName), "builder");
			}

			if (kernel == null)
			{
				afterInit += () => RegisterServiceHostBuilder(serviceHostBuilder, builder, force);
			}
			else
			{
				RegisterServiceHostBuilder(serviceHostBuilder, builder, force);
			}
		}

		private void RegisterServiceHostBuilder(Type serviceHostBuilder, Type builder, bool force)
		{
			if (force || kernel.HasComponent(serviceHostBuilder) == false)
			{
				kernel.Register(Component.For(serviceHostBuilder).ImplementedBy(builder));
			}
		}

		private static IEnumerable<IWcfServiceModel> ResolveServiceModels(ComponentModel model)
		{
			bool foundOne = false;

			if (model.Implementation.IsClass && !model.Implementation.IsAbstract)
			{
				foreach (var serviceModel in WcfUtils.FindDependencies<IWcfServiceModel>(model.CustomDependencies))
				{
					foundOne = true;
					yield return serviceModel;
				}

				if (foundOne == false && model.Configuration != null && "true" == model.Configuration.Attributes[WcfConstants.ServiceHostEnabled])
				{
					yield return new DefaultServiceModel();
				}
			}
		}

		#region CreateServiceHost Members

		public static ServiceHost CreateServiceHost(IKernel Kernel, IWcfServiceModel serviceModel, ComponentModel model, params Uri[] baseAddresses)
		{
			var createServiceHost = createServiceHostCache.GetOrAdd(serviceModel.GetType(), serviceModelType =>
			{
				return (CreateServiceHostDelegate)Delegate.CreateDelegate(typeof(CreateServiceHostDelegate),
						createServiceHostMethod.MakeGenericMethod(serviceModelType));
			});

			return createServiceHost(Kernel, serviceModel, model, baseAddresses);
		}

		internal static ServiceHost CreateServiceHostInternal<M>(IKernel kernel, IWcfServiceModel serviceModel, ComponentModel model, params Uri[] baseAddresses)
			where M : IWcfServiceModel
		{
			var serviceHostBuilder = kernel.Resolve<IServiceHostBuilder<M>>();
			return serviceHostBuilder.Build(model, (M)serviceModel, baseAddresses);
		}

		#endregion

		private void CreateServiceHostWhenHandlerIsValid(IHandler handler, IWcfServiceModel serviceModel, ComponentModel model)
		{
			if (serviceModel.ShouldOpenEagerly.GetValueOrDefault(OpenServiceHostsEagerly) || handler.CurrentState == HandlerState.Valid)
			{
				CreateAndOpenServiceHost(serviceModel, model);
			}
			else
			{
				HandlersChangedDelegate onStateChanged = null;
				onStateChanged = (ref bool stateChanged) =>
				{
					if (handler.CurrentState == HandlerState.Valid && onStateChanged != null)
					{
						kernel.HandlersChanged -= onStateChanged;
						onStateChanged = null;
						CreateAndOpenServiceHost(serviceModel, model);
					}
				};
				kernel.HandlersChanged += onStateChanged;
			}
		}

		private void CreateAndOpenServiceHost(IWcfServiceModel serviceModel, ComponentModel model)
		{
			var serviceHost = CreateServiceHost(kernel, serviceModel, model);
			lock (sync)
			{
				hosts.Add(serviceHost);
			}
			serviceHost.Open();
		}

		private void ReleaseServiceHosts()
		{
			foreach (var serviceHost in hosts)
			{
				foreach (var cleanUp in serviceHost.Extensions.FindAll<IWcfCleanUp>())
				{
					cleanUp.CleanUp();
				}
				WcfUtils.ReleaseCommunicationObject(serviceHost, CloseTimeout);
			}
		}
	}
}
