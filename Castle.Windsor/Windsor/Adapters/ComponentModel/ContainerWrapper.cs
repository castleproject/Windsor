// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Adapters.ComponentModel
{
#if (!SILVERLIGHT)
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.Design;

	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;

	using Component = Castle.MicroKernel.Registration.Component;

	/// <summary>
	/// Implementation of <see cref="IContainerAdapter"/> that does not assume ownership of the
	/// wrapped <see cref="IWindsorContainer"/>. 
	/// </summary>
	public class ContainerWrapper : IContainerAdapter
	{
		#region ContainerWrapper Fields

		private ISite site;
		private readonly IWindsorContainer container;
		private readonly IServiceProvider parentProvider;
		private readonly IList<IContainerAdapterSite> sites =new List<IContainerAdapterSite>();
		private readonly Lock @lock = Lock.Create();

		#endregion

		#region ContainerWrapper Constructors 

		/// <summary>
		/// Constructs an initial ContainerWrapper.
		/// </summary>
		/// <param name="container">The <see cref="IWindsorContainer"/> to adapt.</param>
		public ContainerWrapper(IWindsorContainer container)
			: this(container, null)
		{
			// Empty
		}

		/// <summary>
		/// Constructs an initial ContainerWrapper.
		/// </summary>
		/// <param name="container">The <see cref="IWindsorContainer"/> to adapt.</param>
		/// <param name="parentProvider">The parent <see cref="IServiceProvider"/>.</param>
		public ContainerWrapper(IWindsorContainer container, IServiceProvider parentProvider)
		{
			if (container == null)
			{
				container = CreateDefaultWindsorContainer();
			}

			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			this.container = container;
			this.parentProvider = parentProvider;

			RegisterAdapterWithKernel();
		}

		#endregion

		#region IComponent Members

		/// <summary>
		/// Gets or sets the <see cref="ISite"/> associated with the <see cref="IComponent"/>.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public virtual ISite Site
		{
			get { return site; }
			set { site = value; }
		}

		/// <summary>
		/// Event that notifies the disposal of the <see cref="IComponent"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler Disposed;

		#endregion

		#region IContainer Members

		/// <summary>
		/// Gets all the components in the <see cref="IContainer"/>.
		/// </summary>
		public virtual ComponentCollection Components
		{
			get
			{
				using(@lock.ForReading())
				{
					IComponent[] components = new IComponent[sites.Count];

					for (int i = 0; i < sites.Count; ++i)
					{
						components[i] = sites[i].Component;
					}

					return new ComponentCollection(components);
				}
			}
		}

		/// <summary>
		/// Adds the specified <see cref="IComponent"/> to the <see cref="IContainer"/> at the end of the list.
		/// </summary>
		/// <param name="component">The <see cref="IComponent"/> to add.</param>
		public virtual void Add(IComponent component)
		{
			Add(component, null);
		}

		/// <summary>
		/// Adds the specified <see cref="IComponent"/> to the <see cref="IContainer"/> at the end of the list,
		/// and assigns a name to the component.
		/// </summary>
		/// <param name="component">The <see cref="IComponent"/> to add.</param>
		/// <param name="name">The unique, case-insensitive name to assign to the component, or null.</param>
		public virtual void Add(IComponent component, String name)
		{
			if (component != null)
			{
				using(@lock.ForWriting())
				{
					ISite site = component.Site;

					if ((site == null) || (site.Container != this))
					{
						IContainerAdapterSite newSite = CreateSite(component, name);

						try
						{
						    Kernel.Register(Component.For<IComponent>().Named(newSite.EffectiveName).Instance(component));
						}
						catch (ComponentRegistrationException ex)
						{
							throw new ArgumentException(ex.Message);
						}

						if (site != null)
						{
							site.Container.Remove(component);
						}

						component.Site = newSite;
						sites.Add(newSite);
					}
				}
			}
		}

		/// <summary>
		/// Removes a component from the <see cref="IContainer"/>.
		/// </summary>
		/// <param name="component">The <see cref="IComponent"/> to remove</param>
		public virtual void Remove(IComponent component)
		{
			Remove(component, true);
		}

		private void Remove(IComponent component, bool fromKernel)
		{
			if (component != null)
			{
				using(@lock.ForWriting())
				{
					IContainerAdapterSite site = component.Site as ContainerAdapterSite;

					if (site != null && site.Container == this)
					{
						if (fromKernel)
						{
							if (!Kernel.RemoveComponent(site.EffectiveName))
							{
								throw new ArgumentException("Unable to remove the requested component");
							}
						}

						component.Site = null;

						sites.Remove(site);
					}
				}
			}
		}

		protected virtual IWindsorContainer CreateDefaultWindsorContainer()
		{
			return null;
		}

		protected virtual IContainerAdapterSite CreateSite(IComponent component, String name)
		{
			return new ContainerAdapterSite(component, this, name);
		}

		#endregion

		#region IServiceContainer Members

		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">The type of service.</param>
		/// <returns>An object inplementing service, or null.</returns>
		public virtual object GetService(Type serviceType)
		{
			if(serviceType == null)
			{
				return null;
			}

			// Check for instrinsic services.
			if (serviceType == typeof(IContainerAdapter) ||
				serviceType == typeof(IContainerAccessor) ||
				serviceType == typeof(IServiceContainer) ||
				serviceType == typeof(IContainer))
			{
				return this;
			}
			if( serviceType == typeof(IWindsorContainer))
			{
				return container;
			}
			if (serviceType == typeof(IKernel))
			{
				return Kernel;
			}
			
			// Then, check the Windsor Container.
			try
			{
				return container[serviceType];
			}
			catch (ComponentNotFoundException)
			{
				// Fall through
			}

			// Otherwise, check the parent service provider.
			if (parentProvider != null)
			{
				return parentProvider.GetService(serviceType);
			}

			// Finally, check the chained container.
			if (site != null)
			{
				return site.GetService(serviceType);
			}

			return null;
		}

		/// <summary>
		/// Adds the specified service to the service container.
		/// </summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="serviceInstance">The instance of the service to add.</param>
		public virtual void AddService(Type serviceType, object serviceInstance)
		{
			AddService(serviceType, serviceInstance, false);
		}
		/// <summary>
		/// Adds the specified service to the service container.
		/// </summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="callback">A callback object that is used to create the service.</param>
		public virtual void AddService(Type serviceType, ServiceCreatorCallback callback)
		{
			AddService(serviceType, callback, false);
		}
		/// <summary>
		/// Adds the specified service to the service container, and optionally
		/// promotes the service to any parent service containers.
		/// </summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="serviceInstance">The instance of the service to add.</param>
		/// <param name="promote">true to promote this request to any parent service containers.</param>
		public virtual void AddService(Type serviceType, object serviceInstance, bool promote)
		{
			if (serviceInstance is ServiceCreatorCallback)
			{
				AddService(serviceType, (ServiceCreatorCallback) serviceInstance, promote);
				return;
			}

			if (promote)
			{
				IServiceContainer parentServices = ParentServices;

				if (parentServices != null)
				{
					parentServices.AddService(serviceType, serviceInstance, promote);
					return;
				}
			}

			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}

			if (serviceInstance == null)
			{
				throw new ArgumentNullException("serviceInstance");
			}

			if (!(serviceInstance.GetType().IsCOMObject ||
				serviceType.IsAssignableFrom(serviceInstance.GetType())))
			{
				throw new ArgumentException(String.Format(
					"Invalid service '{0}' for type '{1}'",
					serviceInstance.GetType().FullName, serviceType.FullName));
			}

			if (HasService(serviceType))
			{
				throw new ArgumentException(String.Format(
					"A service for type '{0}' already exists", serviceType.FullName));
			}

			String serviceName = GetServiceName(serviceType);
		    Kernel.Register(Component.For(serviceType).Named(serviceName).Instance(serviceInstance));
		}
		
		/// <summary>
		/// Adds the specified service to the service container, and optionally 
		/// promotes the service to parent service containers.
		/// </summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="callback">A callback object that is used to create the service.</param>
		/// <param name="promote">true to promote this request to any parent service containers.</param>
		public virtual void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
		    if (promote)
		    {
		        IServiceContainer parentServices = ParentServices;

		        if (parentServices != null)
		        {
		            parentServices.AddService(serviceType, callback, promote);
		            return;
		        }
		    }

		    if (serviceType == null)
		    {
		        throw new ArgumentNullException("serviceType");
		    }

		    if (callback == null)
		    {
		        throw new ArgumentNullException("callback");
		    }

		    if (HasService(serviceType))
		    {
		        throw new ArgumentException(String.Format(
		            "A service for type '{0}' already exists", serviceType.FullName),
		                                    "serviceType");
		    }

		    Kernel.Register(MicroKernel.Registration.Component.For(serviceType)
		                        .Named(GetServiceName(serviceType))
		                        .Activator<ServiceCreatorCallbackActivator>()
		                        .ExtendedProperties(
		                            Property.ForKey(ServiceCreatorCallbackActivator.ServiceContainerKey)
		                                .Eq(GetService(typeof(IServiceContainer))),
		                            Property.ForKey(ServiceCreatorCallbackActivator.ServiceCreatorCallbackKey)
		                                .Eq(callback)));
		}

        /// <summary>
		/// Removes the specified service type from the service container.
		/// </summary>
		/// <param name="serviceType">The type of service to remove.</param>
		public virtual void RemoveService(Type serviceType)
		{
			RemoveService(serviceType, false);
		}

		/// <summary>
		/// Removes the specified service type from the service container, 
		/// and optionally promotes the service to parent service containers.
		/// </summary>
		/// <param name="serviceType">The type of service to remove.</param>
		/// <param name="promote">true to promote this request to any parent service containers.</param>
		public virtual void RemoveService(Type serviceType, bool promote)
		{
			if (promote)
			{
				IServiceContainer parentServices = ParentServices;

				if (parentServices != null)
				{
					parentServices.RemoveService(serviceType, promote);
					return;
				}
			}

			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}

			if (IsIntrinsicService(serviceType))
			{
				throw new ArgumentException("Cannot remove an instrinsic service");
			}

			String serviceName = GetServiceName(serviceType);

			if (!Kernel.RemoveComponent(serviceName))
			{
				throw new ArgumentException("Unable to remove the requested service");
			}
		}

		/// <summary>
		/// Determins if the service type represents an intrinsic service.
		/// </summary>
		/// <param name="serviceType">The type of service to remove.</param>
		/// <returns>true if the service type is an intrinsic service.</returns>
		private bool IsIntrinsicService(Type serviceType)
		{
			return serviceType == typeof(IWindsorContainer) ||
				serviceType == typeof(IServiceContainer) ||
				serviceType == typeof(IContainer) ||
				serviceType == typeof(IKernel);
		}

		/// <summary>
		/// Determins if the specified service type exists in the service container.
		/// </summary>
		/// <param name="serviceType">The type of service to remove.</param>
		/// <returns>true if the service type exists.</returns>
		private bool HasService(Type serviceType)
		{
			return IsIntrinsicService(serviceType) ||
				Kernel.HasComponent(serviceType);
		}

		#endregion

		#region IContainerAccessor Members

		/// <summary>
		/// Gets the adapted <see cref="IWindsorContainer"/>
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IWindsorContainer Container
		{
			get { return container; }
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Releases the resources used by the component.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the resources used by the component.
		/// </summary>
		/// <param name="disposing">true if disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{

				using(@lock.ForWriting())
				{
					DisposeContainer();
					DisposeComponent();
					RaiseDisposed();
				}
			}
		}

		private void DisposeComponent()
		{
			if ((site != null) && (site.Container != null))
			{
				site.Container.Remove(this);
			}
		}

		private void DisposeContainer()
		{
			Kernel.ComponentUnregistered -= new ComponentDataDelegate(OnComponentUnregistered);

			for (int i = 0; i < sites.Count; ++i)
			{
				ISite site = (ISite) sites[i];
				site.Component.Site = null;
				site.Component.Dispose();
			}
			sites.Clear();

			InternalDisposeContainer();
		}

		protected virtual void InternalDisposeContainer()
		{
			// Empty
		}

		private void OnComponentUnregistered(String key, IHandler handler)
		{
			IComponent component = handler.Resolve(CreationContext.Empty) as IComponent;

			if (component == this)
			{
				Dispose();
			}
			else if (component != null)
			{
				Remove(component, false);
			}
		}

		private void RaiseDisposed()
		{
			if (Disposed != null)
			{
				Disposed(this, EventArgs.Empty);
				Disposed = null;
			}
		}

		~ContainerWrapper()
		{
			Dispose(false);
		}

		#endregion

		#region ContainerWrapper Members

		private IKernel Kernel
		{
			get { return container.Kernel; }
		}

		private IServiceContainer ParentServices
		{
			get
			{
				IServiceContainer parentServices = null;

				if (parentProvider != null)
				{
					parentServices = (IServiceContainer) parentProvider.GetService(typeof(IServiceContainer));
				}

				if (site != null)
				{
					parentServices = (IServiceContainer) site.GetService(typeof(IServiceContainer));
				}

				return parentServices;
			}
		}

		private void RegisterAdapterWithKernel()
		{
			String adapterName = String.Format("#ContainerAdapter:{0}#", Guid.NewGuid());
			Kernel.Register(Component.For<object>().Named(adapterName).Instance(this));

			Kernel.ComponentUnregistered += OnComponentUnregistered;
		}

		private String GetServiceName(Type serviceType)
		{
			return String.Format("#ContainerAdapterService:{0}#", serviceType.FullName);
		}

		#endregion
	}
#endif
}
