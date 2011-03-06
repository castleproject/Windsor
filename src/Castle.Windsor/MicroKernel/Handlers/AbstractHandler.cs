// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.ModelBuilder.Inspectors;
	using Castle.MicroKernel.Resolvers;

	/// <summary>
	///   Implements the basis of
	///   <see cref = "IHandler" />
	/// </summary>
	[Serializable]
#if SILVERLIGHT
	public abstract class AbstractHandler : IHandler, IExposeDependencyInfo, IDisposable
#else
	public abstract class AbstractHandler : MarshalByRefObject, IHandler, IExposeDependencyInfo, IDisposable
#endif
	{
		/// <summary>
		///   Lifestyle manager instance
		/// </summary>
		protected ILifestyleManager lifestyleManager;

		private readonly ComponentModel model;

		/// <summary>
		///   Custom dependencies values associated with the handler
		/// </summary>
		private IDictionary customParameters;

		/// <summary>
		///   Dictionary of key (string) to
		///   <see cref = "DependencyModel" />
		/// </summary>
		private HashSet<DependencyModel> missingDependencies;

		private IKernelInternal kernel;

		private HandlerState state;

		/// <summary>
		///   Constructs and initializes the handler
		/// </summary>
		/// <param name = "model"></param>
		protected AbstractHandler(ComponentModel model)
		{
			this.model = model;
			state = HandlerState.Valid;
			if (this.model.HasCustomDependencies == false)
			{
				return;
			}
			customParameters = new Arguments(this.model.CustomDependencies);
		}

		/// <summary>
		///   Gets the component model.
		/// </summary>
		public ComponentModel ComponentModel
		{
			get { return model; }
		}

		/// <summary>
		///   Gets the handler state.
		/// </summary>
		public HandlerState CurrentState
		{
			get { return state; }
		}

		public IEnumerable<Type> Services
		{
			get { return ComponentModel.Services; }
		}

		private ICollection<DependencyModel> MissingDependencies
		{
			get
			{
				if (missingDependencies == null)
				{
					missingDependencies = new HashSet<DependencyModel>();
				}
				return missingDependencies;
			}
		}

		protected IKernelInternal Kernel
		{
			get { return kernel; }
		}

		/// <summary>
		///   Should be implemented by derived classes: 
		///   disposes the component instance (or recycle it)
		/// </summary>
		/// <param name = "burden"></param>
		/// <returns>true if destroyed.</returns>
		public abstract bool ReleaseCore(Burden burden);

		public virtual void Dispose()
		{
			lifestyleManager.Dispose();
		}

		public void AddCustomDependencyValue(object key, object value)
		{
			if (customParameters == null)
			{
				customParameters = new Arguments();
			}

			customParameters[key] = value;
			RaiseHandlerStateChanged();
		}

		public bool HasCustomParameter(object key)
		{
			if (key == null)
			{
				return false;
			}

			if (customParameters == null)
			{
				return false;
			}

			return customParameters.Contains(key);
		}

		/// <summary>
		///   Saves the kernel instance, subscribes to
		///   <see cref = "IKernelEvents.AddedAsChildKernel" />
		///   event,
		///   creates the lifestyle manager instance and computes
		///   the handler state.
		/// </summary>
		/// <param name = "kernel"></param>
		public virtual void Init(IKernelInternal kernel)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			this.kernel = kernel;
			this.kernel.AddedAsChildKernel += OnAddedAsChildKernel;

			var activator = this.kernel.CreateComponentActivator(ComponentModel);
			lifestyleManager = CreateLifestyleManager(activator);
			EnsureDependenciesCanBeSatisfied(activator as IDependencyAwareActivator);

			if (AllRequiredDependenciesResolvable())
			{
				SetNewState(HandlerState.Valid);
				DisconnectEvents();
				missingDependencies = null;
			}
		}

		public bool IsBeingResolvedInContext(CreationContext context)
		{
			return context != null && context.IsInResolutionContext(this);
		}

		/// <summary>
		///   disposes the component instance (or recycle it).
		/// </summary>
		/// <param name = "burden"></param>
		/// <returns></returns>
		public virtual bool Release(Burden burden)
		{
			return ReleaseCore(burden);
		}

		public void RemoveCustomDependencyValue(object key)
		{
			if (customParameters != null)
			{
				customParameters.Remove(key);
				RaiseHandlerStateChanged();
			}
		}

		/// <summary>
		///   Returns an instance of the component this handler
		///   is responsible for
		/// </summary>
		/// <param name = "context"></param>
		/// <returns></returns>
		public object Resolve(CreationContext context)
		{
			return Resolve(context, true);
		}

		public object TryResolve(CreationContext context)
		{
			try
			{
				return Resolve(context, false);
			}
			catch (DependencyResolverException)
			{
				// this exception is thrown when a dependency can not be resolved
				// in which case we're free to ignore it and fallback
				// TODO: this should be logged
				return null;
			}
			catch (HandlerException)
			{
				// this exception is thrown when Handler can't operate or in the same
				// cases as above, which means we should throw DependencyResolverException
				// instead (in vNext, not to break anyone now)
				// TODO: this should be logged
				return null;
			}
			catch (NoResolvableConstructorFoundException)
			{
				// it's pretty obvious when this exception is thrown methinks.
				// this was added as temporary (how many times have you heard someone say that)
				// workaround for issue IOC-239
				return null;
			}
		}

		public virtual bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			if (customParameters == null || customParameters.Count == 0)
			{
				return false;
			}
			return HasCustomParameter(dependency.DependencyKey) || HasCustomParameter(dependency.TargetItemType);
		}

		public virtual object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			Debug.Assert(CanResolve(context, contextHandlerResolver, model, dependency), "CanResolve(context, contextHandlerResolver, model, dependency)");
			if (HasCustomParameter(dependency.DependencyKey))
			{
				return customParameters[dependency.DependencyKey];
			}

			return customParameters[dependency.TargetItemType];
		}

		/// <summary>
		///   Invoked by
		///   <see cref = "EnsureDependenciesCanBeSatisfied" />
		///   in order to check if a dependency can be satisfied.
		///   If not, the handler is set to a 'waiting dependency' state.
		/// </summary>
		/// <remarks>
		///   This method registers the dependencies within the correct collection 
		///   or dictionary and changes the handler state to
		///   <see cref = "HandlerState.WaitingDependency" />
		/// </remarks>
		/// <param name = "dependency"></param>
		protected void AddDependency(DependencyModel dependency)
		{
			dependency.Init(model.ParametersInternal);
			if (AddOptionalDependency(dependency))
			{
				return;
			}
			if (AddResolvableDependency(dependency))
			{
				return;
			}
			AddMissingDependency(dependency);
		}

		private bool AddOptionalDependency(DependencyModel dependency)
		{
			if (dependency.IsOptional || dependency.HasDefaultValue)
			{
				AddGraphDependency(dependency);
				return true;
			}
			return false;
		}

		private bool AddResolvableDependency(DependencyModel dependency)
		{
			if (HasValidComponentFromResolver(dependency))
			{
				AddGraphDependency(dependency);
				return true;
			}
			return false;
		}

		private void AddGraphDependency(DependencyModel dependency)
		{
			var handler = dependency.GetHandler(Kernel);
			if (handler != null)
			{
				ComponentModel.AddDependent(handler.ComponentModel);
			}
		}

		protected void AddMissingDependency(DependencyModel dependency)
		{
			MissingDependencies.Add(dependency);
			if (state != HandlerState.WaitingDependency)
			{
				// This handler is considered invalid
				// until dependencies are satisfied
				SetNewState(HandlerState.WaitingDependency);

				// Register itself on the kernel
				// to be notified if the dependency is satified
				Kernel.HandlersChanged += DependencySatisfied;

				// We also gonna pay attention for state
				// changed within this very handler. The 
				// state can be changed by AddCustomDependencyValue and RemoveCustomDependencyValue
				OnHandlerStateChanged += HandlerStateChanged;
			}
		}

		/// <summary>
		///   Creates an implementation of
		///   <see cref = "ILifestyleManager" />
		///   based
		///   on
		///   <see cref = "LifestyleType" />
		///   and invokes
		///   <see cref = "ILifestyleManager.Init" />
		///   to initialize the newly created manager.
		/// </summary>
		/// <param name = "activator"></param>
		/// <returns></returns>
		protected virtual ILifestyleManager CreateLifestyleManager(IComponentActivator activator)
		{
			ILifestyleManager manager;
			var type = ComponentModel.LifestyleType;

			switch (type)
			{
				case LifestyleType.Thread:
#if SILVERLIGHT
					manager = new PerThreadThreadStaticLifestyleManager();
#else
					manager = new PerThreadLifestyleManager();
#endif
					break;
				case LifestyleType.Transient:
					manager = new TransientLifestyleManager();
					break;
#if (!SILVERLIGHT && !CLIENTPROFILE)
				case LifestyleType.PerWebRequest:
					manager = new PerWebRequestLifestyleManager();
					break;
#endif
				case LifestyleType.Custom:
					manager = ComponentModel.CustomLifestyle.CreateInstance<ILifestyleManager>();

					break;
				case LifestyleType.Pooled:
				{
					var initial = ExtendedPropertiesConstants.Pool_Default_InitialPoolSize;
					var maxSize = ExtendedPropertiesConstants.Pool_Default_MaxPoolSize;

					if (ComponentModel.ExtendedProperties.Contains(ExtendedPropertiesConstants.Pool_InitialPoolSize))
					{
						initial = (int)ComponentModel.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize];
					}
					if (ComponentModel.ExtendedProperties.Contains(ExtendedPropertiesConstants.Pool_MaxPoolSize))
					{
						maxSize = (int)ComponentModel.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize];
					}

					manager = new PoolableLifestyleManager(initial, maxSize);
				}
					break;
				default:
					//this includes LifestyleType.Undefined, LifestyleType.Singleton and invalid values
					manager = new SingletonLifestyleManager();
					break;
			}

			manager.Init(activator, Kernel, model);

			return manager;
		}

		/// <summary>
		///   Invoked by the kernel
		///   when one of registered dependencies were satisfied by 
		///   new components registered.
		/// </summary>
		/// <remarks>
		///   Handler for the event
		///   <see cref = "IKernelEvents.HandlerRegistered" />
		/// </remarks>
		/// <param name = "stateChanged"></param>
		protected void DependencySatisfied(ref bool stateChanged)
		{
			foreach (var dependency in MissingDependencies.ToArray())
			{
				if (HasCustomParameter(dependency.DependencyKey))
				{
					MissingDependencies.Remove(dependency);
				}
			}

			// Check within the Kernel
			foreach (var dependency in MissingDependencies.ToArray())
			{
				var service = dependency.TargetItemType;
				if (service != null && HasValidComponent(service, dependency))
				{
					MissingDependencies.Remove(dependency);
					var dependingHandler = kernel.GetHandler(service);
					if (dependingHandler != null) //may not be real handler, if comes from resolver
					{
						AddGraphDependency(dependingHandler);
					}
				}
				else
				{
					var name = dependency.DependencyKey;
					if (name != null && (HasValidComponent(name, dependency) || HasCustomParameter(name)))
					{
						MissingDependencies.Remove(dependency);
						var dependingHandler = kernel.GetHandler(name);
						if (dependingHandler != null) //may not be real handler, if we are using sub resolver
						{
							AddGraphDependency(dependingHandler);
						}
					}
				}
			}

			if (AllRequiredDependenciesResolvable())
			{
				SetNewState(HandlerState.Valid);
				stateChanged = true;

				DisconnectEvents();

				// We don't need these anymore
				missingDependencies = null;
			}
		}

		private bool AllRequiredDependenciesResolvable()
		{
			if (MissingDependencies.Count == 0)
			{
				return true;
			}
			var constructorDependencies = MissingDependencies.Where(d => d is ConstructorDependencyModel)
				.Cast<ConstructorDependencyModel>().ToList();
			if (MissingDependencies.Count != constructorDependencies.Count)
			{
				return false;
			}

			var ctorDependenciesByCtor = constructorDependencies.ToLookup(d => d.Constructor);
			return model.Constructors.Count > ctorDependenciesByCtor.Count;
		}

		/// <summary>
		///   Checks if the handler is able to, at very least, satisfy
		///   the dependencies for the constructor with less parameters
		/// </summary>
		/// <remarks>
		///   For each non*optional dependency, the implementation will invoke
		///   <see cref = "AddDependency" />
		/// </remarks>
		protected virtual void EnsureDependenciesCanBeSatisfied(IDependencyAwareActivator activator)
		{
			if (activator != null && activator.CanProvideRequiredDependencies(ComponentModel))
			{
				return;
			}

			// Property dependencies may not be optional

			foreach (var property in ComponentModel.Properties)
			{
				var dependency = property.Dependency;

				AddDependency(dependency);
			}

			// The following dependencies were added by - for example - 
			// facilities, for some reason, and we need to satisfy the non-optional

			foreach (var dependency in ComponentModel.Dependencies)
			{
				AddDependency(dependency);
			}
			var constructorsCount = ComponentModel.Constructors.Count;
			if (constructorsCount == 0)
			{
				return;
			}
			foreach (var constructor in ComponentModel.Constructors)
			{
				foreach (var dependency in constructor.Dependencies)
				{
					AddDependency(dependency);
				}
			}
		}

		/// <summary>
		///   Invoked when the container receives a parent container reference.
		/// </summary>
		/// <remarks>
		///   This method implementation checks whether the parent container
		///   is able to supply the dependencies for this handler.
		/// </remarks>
		/// <param name = "sender"></param>
		/// <param name = "e"></param>
		protected void OnAddedAsChildKernel(object sender, EventArgs e)
		{
			if (MissingDependencies.Count == 0)
			{
				return;
			}
			var stateChanged = false;
			if (MissingDependencies.Any(d => d.GetHandler(Kernel.Parent) != null))
			{
				DependencySatisfied(ref stateChanged);
			}
		}

		/// <summary>
		///   Returns an instance of the component this handler
		///   is responsible for
		/// </summary>
		/// <param name = "context"></param>
		/// <param name = "instanceRequired">when <c>false</c>, handler can not create valid instance and return <c>null</c> instead </param>
		/// <returns></returns>
		protected abstract object Resolve(CreationContext context, bool instanceRequired);

		protected void SetNewState(HandlerState newState)
		{
			state = newState;
		}

		private void AddGraphDependency(IHandler handler)
		{
			ComponentModel.AddDependent(handler.ComponentModel);
		}

		private void DisconnectEvents()
		{
			Kernel.HandlersChanged -= DependencySatisfied;
			Kernel.AddedAsChildKernel -= OnAddedAsChildKernel;
			OnHandlerStateChanged -= HandlerStateChanged;
		}

		/// <summary>
		///   Handler for the event
		///   <see cref = "OnHandlerStateChanged" />
		/// </summary>
		/// <param name = "source"></param>
		/// <param name = "args"></param>
		private void HandlerStateChanged(object source, EventArgs args)
		{
			Kernel.RaiseHandlerRegistered(this);
			Kernel.RaiseHandlersChanged();
		}

		private bool HasValidComponent(Type service, DependencyModel dependency)
		{
			var handlr = kernel.GetHandler(service);
			if (handlr == null)
			{
				return false;
			}
			if (IsValidHandlerState(handlr))
			{
				return true;
			}

			foreach (var handler in kernel.GetHandlers(service))
			{
				if (IsValidHandlerState(handler))
				{
					return true;
				}
			}

			// could not find in kernel directly, check using resolvers
			return HasValidComponentFromResolver(dependency);
		}

		private bool HasValidComponent(String key, DependencyModel dependency)
		{
			if (IsValidHandlerState(kernel.GetHandler(key)))
			{
				return true;
			}
			// could not find in kernel directly, check using resolvers
			return HasValidComponentFromResolver(dependency);
		}

		private bool HasValidComponentFromResolver(DependencyModel dependency)
		{
			return Kernel.Resolver.CanResolve(null, this, model, dependency);
		}

		private void InitializeCustomDependencies()
		{
			if (model.HasCustomDependencies == false)
			{
				return;
			}
			customParameters = new Arguments();
			foreach (DictionaryEntry customParameter in model.CustomDependencies)
			{
				customParameters.Add(customParameter.Key, customParameter.Value);
			}
		}

		private bool IsValidHandlerState(IHandler handler)
		{
			if (handler == null)
			{
				return false;
			}

			return handler.CurrentState == HandlerState.Valid;
		}

		private void RaiseHandlerStateChanged()
		{
			if (OnHandlerStateChanged != null)
			{
				OnHandlerStateChanged(this, EventArgs.Empty);
			}
		}

		public event HandlerStateDelegate OnHandlerStateChanged;

		public override string ToString()
		{
			return string.Format("Model: {0}", model);
		}

		public void ObtainDependencyDetails(IDependencyInspector inspector)
		{
			if (CurrentState == HandlerState.Valid)
			{
				return;
			}
			inspector.Inspect(this, MissingDependencies.ToArray(), Kernel);
		}

		protected bool CanResolvePendingDependencies(CreationContext context)
		{
			if (CurrentState == HandlerState.Valid)
			{
				return true;
			}
			// detect circular dependencies
			if (IsBeingResolvedInContext(context))
			{
				return context.HasAdditionalArguments;
			}
			var canResolveAll = true;
			foreach (var dependency in MissingDependencies.ToArray())
			{
				if (dependency.TargetItemType == null)
				{
					canResolveAll = false;
					break;
				}
				// a self-dependency is not allowed
				var handler = Kernel.LoadHandlerByType(dependency.DependencyKey, dependency.TargetItemType, context.AdditionalArguments);
				if (handler == this || handler == null)
				{
					canResolveAll = false;
					break;
				}
			}
			return canResolveAll || context.HasAdditionalArguments;
		}
	}
}