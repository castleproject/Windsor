// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
	using System.Reflection;
	using System.Text;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.ModelBuilder.Inspectors;

	public delegate ComponentReleasingDelegate ComponentResolvingDelegate(IKernel kernel, CreationContext context);

	public delegate void ComponentReleasingDelegate(IKernel kernel);

	/// <summary>
	///   Implements the basis of
	///   <see cref = "IHandler" />
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	[DebuggerDisplay("Model: {ComponentModel.Service} / {ComponentModel.Implementation} ")]
#if (!SILVERLIGHT)
	public abstract class AbstractHandler : MarshalByRefObject, IHandler, IExposeDependencyInfo, IDisposable
#else
	public abstract class AbstractHandler : IHandler, IExposeDependencyInfo, IDisposable
#endif
	{
		private readonly ComponentModel model;
		private readonly ComponentResolvingDelegate resolvingHandler;

		/// <summary>
		///   Custom dependencies values associated with the handler
		/// </summary>
		private IDictionary customParameters;

		/// <summary>
		///   Dictionary of key (string) to
		///   <see cref = "DependencyModel" />
		/// </summary>
		private IDictionary<string, DependencyModel> dependenciesByKey;

		/// <summary>
		///   Dictionary of Type to a list of
		///   <see cref = "DependencyModel" />
		/// </summary>
		private IDictionary<Type, DependencyModel> dependenciesByService;

		private IKernelInternal kernel;

		/// <summary>
		///   Lifestyle manager instance
		/// </summary>
		protected ILifestyleManager lifestyleManager;

		private IDictionary<object, IList<ComponentReleasingDelegate>> releasingHandlers;
		private Type service;
		private HandlerState state;

		/// <summary>
		///   Constructs and initializes the handler
		/// </summary>
		/// <param name = "model"></param>
		public AbstractHandler(ComponentModel model)
		{
			this.model = model;
			state = HandlerState.Valid;
			InitializeCustomDependencies();
			resolvingHandler = model.ExtendedProperties["component_resolving_handler"] as ComponentResolvingDelegate;
		}

		/// <summary>
		///   Gets the handler state.
		/// </summary>
		public HandlerState CurrentState
		{
			get { return state; }
		}

		/// <summary>
		///   Gets the component model.
		/// </summary>
		public ComponentModel ComponentModel
		{
			get { return model; }
		}

		public Type Service
		{
			get
			{
				if (service == null)
					service = ComponentModel.Service;
				return service;
			}
		}

		protected IKernelInternal Kernel
		{
			get { return kernel; }
		}

		protected IDictionary<Type, DependencyModel> DependenciesByService
		{
			get
			{
				if (dependenciesByService == null)
					dependenciesByService = new Dictionary<Type, DependencyModel>();
				return dependenciesByService;
			}
		}

		protected IDictionary<string, DependencyModel> DependenciesByKey
		{
			get
			{
				if (dependenciesByKey == null)
					dependenciesByKey = new Dictionary<string, DependencyModel>(StringComparer.OrdinalIgnoreCase);
				return dependenciesByKey;
			}
		}

		/// <summary>
		///   Saves the kernel instance, subscribes to
		///   <see cref = "IKernelEvents.AddedAsChildKernel" />
		///   event,
		///   creates the lifestyle manager instance and computes
		///   the handler state.
		/// </summary>
		/// <param name = "kernel"></param>
		public virtual void Init(IKernel kernel)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			this.kernel = kernel as IKernelInternal;
			if(this.kernel == null)
			{
				throw new HandlerException(
					string.Format("The kernel does not implement {0}. It must also provide contract for internal usage.",
					              typeof(IKernelInternal).FullName));
			}
			this.kernel.AddedAsChildKernel += OnAddedAsChildKernel;

			var activator = this.kernel.CreateComponentActivator(ComponentModel);
			lifestyleManager = CreateLifestyleManager(activator);
			EnsureDependenciesCanBeSatisfied(activator as IDependencyAwareActivator);

			if (state == HandlerState.Valid)
				DisconnectEvents();
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

		/// <summary>
		/// Returns an instance of the component this handler
		/// is responsible for
		/// </summary>
		/// <param name="context"></param>
		/// <param name="instanceRequired">when <c>false</c>, handler can not create valid instance and return <c>null</c> instead </param>
		/// <returns></returns>
		private object Resolve(CreationContext context, bool instanceRequired)
		{
			List<ComponentReleasingDelegate> releasers = null;

			if (resolvingHandler != null)
			{
				foreach (ComponentResolvingDelegate resolver in resolvingHandler.GetInvocationList())
				{
					var releaser = resolver(kernel, context);

					if (releaser != null)
					{
						if (releasers == null)
							releasers = new List<ComponentReleasingDelegate>();
						releasers.Add(releaser);
					}
				}
			}

			var instance = ResolveCore(context, releasers != null, instanceRequired);

			if (releasers != null)
			{
				lock (resolvingHandler)
				{
					if (releasingHandlers == null)
						releasingHandlers = new Dictionary<object, IList<ComponentReleasingDelegate>>();

					if (releasingHandlers.ContainsKey(instance) == false)
						releasingHandlers.Add(instance, releasers);
				}
			}

			return instance;
		}

		public object TryResolve(CreationContext context)
		{
			return Resolve(context, false);
		}

		/// <summary>
		///   Should be implemented by derived classes: 
		///   returns an instance of the component this handler
		///   is responsible for
		/// </summary>
		/// <param name = "context"></param>
		/// <param name = "track"></param>
		/// <param name="instanceRequired">When <c>false</c>, handler can not create valid instance and return <c>null</c> instead.</param>
		/// <returns></returns>
		protected abstract object ResolveCore(CreationContext context, bool track, bool instanceRequired);

		/// <summary>
		///   disposes the component instance (or recycle it).
		/// </summary>
		/// <param name = "instance"></param>
		/// <returns></returns>
		public bool Release(object instance)
		{
			if (releasingHandlers != null)
			{
				IList<ComponentReleasingDelegate> releasers;

				lock (releasingHandlers)
				{
					if (releasingHandlers.TryGetValue(instance, out releasers))
						releasingHandlers.Remove(instance);
				}

				if (releasers != null)
				{
					foreach (var releaser in releasers)
					{
						releaser(kernel);
					}
				}
			}

			return ReleaseCore(instance);
		}

		/// <summary>
		///   Should be implemented by derived classes: 
		///   disposes the component instance (or recycle it)
		/// </summary>
		/// <param name = "instance"></param>
		/// <returns>true if destroyed.</returns>
		public abstract bool ReleaseCore(object instance);

		/// <summary>
		///   TODO: Pendent
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "value"></param>
		public void AddCustomDependencyValue(object key, object value)
		{
			if (customParameters == null)
				customParameters = new Arguments();

			customParameters[key] = value;
			RaiseHandlerStateChanged();
		}

		/// <summary>
		///   TODO: Pendent
		/// </summary>
		/// <param name = "key"></param>
		/// <returns></returns>
		public bool HasCustomParameter(object key)
		{
			if (key == null)
				return false;

			if (customParameters == null)
				return false;

			return customParameters.Contains(key);
		}

		/// <summary>
		///   TODO: Pendent
		/// </summary>
		/// <param name = "key"></param>
		public void RemoveCustomDependencyValue(object key)
		{
			if (customParameters != null)
			{
				customParameters.Remove(key);
				RaiseHandlerStateChanged();
			}
		}

		public bool IsBeingResolvedInContext(CreationContext context)
		{
			return context != null && context.IsInResolutionContext(this);
		}

		/// <summary>
		/// </summary>
		public event HandlerStateDelegate OnHandlerStateChanged;

		public virtual object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                              ComponentModel model, DependencyModel dependency)
		{
			if (HasCustomParameter(dependency.DependencyKey))
				return customParameters[dependency.DependencyKey];

			return customParameters[dependency.TargetType];
		}

		public virtual bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                               ComponentModel model, DependencyModel dependency)
		{
			return HasCustomParameter(dependency.DependencyKey) || HasCustomParameter(dependency.TargetType);
		}

		public virtual void Dispose()
		{
			lifestyleManager.Dispose();
		}

		/// <summary>
		///   Returns human readable list of dependencies 
		///   this handler is waiting for.
		/// </summary>
		/// <returns></returns>
		public String ObtainDependencyDetails(IList dependenciesChecked)
		{
			if (CurrentState == HandlerState.Valid)
				return String.Empty;

			if (dependenciesChecked.Contains(this))
				return String.Empty;

			dependenciesChecked.Add(this);

			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{0} is waiting for the following dependencies: ", ComponentModel.Name);
			sb.AppendLine();
			if (DependenciesByService.Count != 0)
			{
				sb.AppendLine();
				sb.AppendLine("Services: ");

				foreach (Type type in DependenciesByService.Keys)
				{
					IHandler handler = Kernel.GetHandler(type);

					if (handler == null)
					{
						sb.AppendFormat("- {0} which was not registered. ", type.FullName ?? type.Name);
						sb.AppendLine();
					}
					else if (handler == this)
					{
						sb.AppendFormat("- {0}. {1}  A dependency cannot be satisfied by itself, " +
						                "did you forget to add a parameter name to differentiate between the " +
						                "two dependencies? ", type.FullName, Environment.NewLine);
						sb.AppendLine();
						foreach (IHandler maybeDecoratedHandler in kernel.GetHandlers(handler.Service))
						{
							if (maybeDecoratedHandler == this)
								continue;
							sb.AppendLine();
							sb.AppendFormat(
								"{0} is registered and is matching the required service, but cannot be resolved.",
								maybeDecoratedHandler.ComponentModel.Name);
							sb.AppendLine();
							IExposeDependencyInfo info = maybeDecoratedHandler as IExposeDependencyInfo;

							if (info != null)
								sb.Append(info.ObtainDependencyDetails(dependenciesChecked));
						}
					}
					else
					{
						sb.AppendFormat("- {0} which was registered but is also waiting for dependencies. ", type.FullName);
						sb.AppendLine();

						IExposeDependencyInfo info = handler as IExposeDependencyInfo;

						if (info != null)
							sb.Append(info.ObtainDependencyDetails(dependenciesChecked));
					}
				}
			}

			if (DependenciesByKey.Count != 0)
			{
				sb.AppendLine();
				sb.AppendLine("Keys (components with specific keys)");

				foreach (var dependency in DependenciesByKey)
				{
					String key = dependency.Key;

					IHandler handler = Kernel.GetHandler(key);

					if (handler == null)
					{
						sb.AppendFormat("- {0} which was not registered. ", key);
						sb.AppendLine();
					}
					else
					{
						sb.AppendFormat("- {0} which was registered but is also waiting for dependencies.", key);
						sb.AppendLine();

						IExposeDependencyInfo info = handler as IExposeDependencyInfo;

						if (info != null)
							sb.Append(info.ObtainDependencyDetails(dependenciesChecked));
					}
				}
			}

			return sb.ToString();
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
			ILifestyleManager manager = null;

			LifestyleType type = ComponentModel.LifestyleType;

			switch (type)
			{
				case LifestyleType.Thread:
#if (!SILVERLIGHT)
					manager = new PerThreadLifestyleManager();
#else
					manager = new PerThreadThreadStaticLifestyleManager();
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
					manager = ReflectionUtil.CreateInstance<ILifestyleManager>(ComponentModel.CustomLifestyle);
					break;
				case LifestyleType.Pooled:
				{
					int initial = ExtendedPropertiesConstants.Pool_Default_InitialPoolSize;
					int maxSize = ExtendedPropertiesConstants.Pool_Default_MaxPoolSize;

					if (ComponentModel.ExtendedProperties.Contains(ExtendedPropertiesConstants.Pool_InitialPoolSize))
						initial = (int)ComponentModel.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize];
					if (ComponentModel.ExtendedProperties.Contains(ExtendedPropertiesConstants.Pool_MaxPoolSize))
						maxSize = (int)ComponentModel.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize];

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
		///   Checks if the handler is able to, at very least, satisfy
		///   the dependencies for the constructor with less parameters
		/// </summary>
		/// <remarks>
		///   For each non*optional dependency, the implementation will invoke
		///   <see cref = "AddDependency" />
		/// </remarks>
		protected virtual void EnsureDependenciesCanBeSatisfied(IDependencyAwareActivator activator)
		{
			if(activator != null && activator.CanProvideRequiredDependencies(ComponentModel))
				return;
			
			// Custom activators should deal with this case
			if (ComponentModel.Constructors.Count == 0)
				return;

			// Property dependencies may not be optional

			foreach (PropertySet property in ComponentModel.Properties)
			{
				DependencyModel dependency = property.Dependency;

				if (dependency.IsOptional == false &&
				    (dependency.DependencyType == DependencyType.Service ||
				     dependency.DependencyType == DependencyType.ServiceOverride))
					AddDependency(dependency);
			}

			// The following dependencies were added by - for example - 
			// facilities, for some reason, and we need to satisfy the non-optional

			foreach (DependencyModel dependency in ComponentModel.Dependencies)
			{
				if (dependency.IsOptional == false &&
				    (dependency.DependencyType == DependencyType.Service ||
				     dependency.DependencyType == DependencyType.ServiceOverride))
					AddDependency(dependency);
			}

			foreach (DependencyModel dependency in GetSecuredDependencies())
			{
				if (dependency.DependencyType == DependencyType.Service ||
				    dependency.DependencyType == DependencyType.ServiceOverride)
					AddDependency(dependency);
				else if (dependency.DependencyType == DependencyType.Parameter &&
				         !ComponentModel.Constructors.HasAmbiguousFewerArgumentsCandidate &&
				         !ComponentModel.Parameters.Contains(dependency.DependencyKey))
					AddDependency(dependency);
			}
		}

		private IEnumerable<DependencyModel> GetSecuredDependencies()
		{
			var candidateConstructors = GetCandidateConstructors();

			// if there is more than one possible constructors, we need to verify
			// its dependencies at resolve time

			if (candidateConstructors.Count() != 1)
				return new DependencyModel[0];

			return candidateConstructors.Single().Dependencies;
		}

		private IEnumerable<ConstructorCandidate> GetCandidateConstructors()
		{
			if (ComponentModel.Constructors.HasAmbiguousFewerArgumentsCandidate == false && CanSatisfyConstructor(ComponentModel.Constructors.FewerArgumentsCandidate))
				return new[] { ComponentModel.Constructors.FewerArgumentsCandidate };

			var candidates = ComponentModel.Constructors
				.Where(CanSatisfyConstructor)
				.GroupBy(c => c.Constructor.GetParameters().Length)
				.ToList();
			if (candidates.Count == 0)
			{
				return ComponentModel.Constructors;
			}

			if (candidates[0].Count() == 1)
				return candidates[0];

			return new[] { SelectMostValuableCandidate(candidates[0]) };
		}

		private ConstructorCandidate SelectMostValuableCandidate(IEnumerable<ConstructorCandidate> candidates)
		{
			// let's try to get one with the most parameter dependencies:
			var bestCandidates = GetBestCandidatesByDependencyType(candidates);
			if (bestCandidates.Count == 1)
				return bestCandidates[0];
			return SelectMostValuableCandidateByName(bestCandidates);
		}

		private ConstructorCandidate SelectMostValuableCandidateByName(IEnumerable<ConstructorCandidate> candidates)
		{
			IEnumerable<KeyValuePair<ConstructorCandidate, ParameterInfo[]>> parametersByConstructor =
				candidates.ToDictionary(c => c,
				                        c => c.Constructor.GetParameters());
			var parametersCount = parametersByConstructor.First().Value.Length;
			for (var i = 0; i < parametersCount; i++)
			{
				var index = i;
				var first = parametersByConstructor.GroupBy(p => p.Value[index].Name).OrderBy(g => g.Key).First();
				if (first.Count() == 1)
					return first.Single().Key;

				parametersByConstructor = first;
			}

			// we have more than one constructor with all parameters with identical names...
			// highly unlikely but...
			// in this case let's just toss a coin
			return parametersByConstructor.First().Key;
		}

		private List<ConstructorCandidate> GetBestCandidatesByDependencyType(IEnumerable<ConstructorCandidate> candidates)
		{
			var parametersCount = 0;
			var bestCandidates = new List<ConstructorCandidate>();
			foreach (var candidate in candidates)
			{
				var count = candidate.Dependencies.Count(d => d.DependencyType == DependencyType.Parameter ||
				                                              d.DependencyType == DependencyType.ServiceOverride);
				if (count < parametersCount) continue;

				if (count == parametersCount)
				{
					bestCandidates.Add(candidate);
					continue;
				}

				bestCandidates.Clear();
				bestCandidates.Add(candidate);
				parametersCount = count;
			}

			return bestCandidates;
		}

		protected bool CanSatisfyConstructor(ConstructorCandidate constructor)
		{
			return constructor.Dependencies.All(CanSatisfyDependency);
		}

		protected bool CanSatisfyDependency(DependencyModel dependency)
		{
			if (HasValidComponentFromResolver(dependency)) return true;

			if (dependency.DependencyType == DependencyType.Service &&
			    dependency.TargetType != null &&
			    DependenciesByService.ContainsKey(dependency.TargetType))
				return true;

			return false;
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
			if (HasValidComponentFromResolver(dependency))
			{
				if (dependency.DependencyType == DependencyType.Service && dependency.TargetType != null)
				{
					IHandler depHandler = Kernel.GetHandler(dependency.TargetType);

					if (depHandler != null)
						AddGraphDependency(depHandler.ComponentModel);
				}
				else
				{
					IHandler depHandler = Kernel.GetHandler(dependency.DependencyKey);

					if (depHandler != null)
						AddGraphDependency(depHandler.ComponentModel);
				}

				return;
			}

			if (dependency.DependencyType == DependencyType.Service && dependency.TargetType != null)
			{
				if (DependenciesByService.ContainsKey(dependency.TargetType))
					return;

				DependenciesByService.Add(dependency.TargetType, dependency);
			}
			else if (!DependenciesByKey.ContainsKey(dependency.DependencyKey))
				DependenciesByKey.Add(dependency.DependencyKey, dependency);

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
			// Check within the handler

			if (customParameters != null && customParameters.Count != 0)
			{
				DependencyModel[] dependencies = Union(DependenciesByService.Values, DependenciesByKey.Values);

				foreach (DependencyModel dependency in dependencies)
				{
					if (!HasCustomParameter(dependency.DependencyKey))
						continue;

					if (dependency.DependencyType == DependencyType.Service)
						DependenciesByService.Remove(dependency.TargetType);
					else
						DependenciesByKey.Remove(dependency.DependencyKey);
				}
			}

			// Check within the Kernel

			foreach (var pair in new Dictionary<Type, DependencyModel>(DependenciesByService))
			{
				Type service = pair.Key;
				DependencyModel dependency = pair.Value;
				if (HasValidComponent(service, dependency))
				{
					DependenciesByService.Remove(service);
					IHandler dependingHandler = kernel.GetHandler(service);
					if (dependingHandler != null) //may not be real handler, if comes from resolver
						AddGraphDependency(dependingHandler.ComponentModel);
				}
			}

			foreach (var pair in new Dictionary<string, DependencyModel>(DependenciesByKey))
			{
				string key = pair.Key;
				DependencyModel dependency = pair.Value;
				if (HasValidComponent(key, dependency) || HasCustomParameter(key))
				{
					DependenciesByKey.Remove(key);
					IHandler dependingHandler = kernel.GetHandler(key);
					if (dependingHandler != null) //may not be real handler, if we are using sub resovler
						AddGraphDependency(dependingHandler.ComponentModel);
				}
			}

			if (DependenciesByService.Count == 0 && DependenciesByKey.Count == 0)
			{
				SetNewState(HandlerState.Valid);
				stateChanged = true;

				DisconnectEvents();

				// We don't need these anymore

				dependenciesByKey = null;
				dependenciesByService = null;
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
			if (DependenciesByKey.Count == 0 && DependenciesByService.Count == 0)
				return;
			bool shouldExecuteDependencyChanged = false;
			bool stateChanged = false;

			Type[] services = new Type[DependenciesByService.Count];

			DependenciesByService.Keys.CopyTo(services, 0);

			foreach (Type service in services)
			{
				if (Kernel.Parent.HasComponent(service))
					shouldExecuteDependencyChanged = true;
			}

			String[] keys = new String[DependenciesByKey.Count];

			DependenciesByKey.Keys.CopyTo(keys, 0);

			foreach (String key in keys)
			{
				if (Kernel.Parent.HasComponent(key))
					shouldExecuteDependencyChanged = true;
			}

			if (shouldExecuteDependencyChanged)
				DependencySatisfied(ref stateChanged);
		}

		protected void SetNewState(HandlerState newState)
		{
			state = newState;
		}

		private void InitializeCustomDependencies()
		{
			customParameters = new Arguments();

			foreach (DictionaryEntry customParameter in model.CustomDependencies)
			{
				customParameters.Add(customParameter.Key, customParameter.Value);
			}
		}

		private bool HasValidComponent(Type service, DependencyModel dependency)
		{
			foreach (IHandler handler in kernel.GetHandlers(service))
			{
				if (IsValidHandlerState(handler))
					return true;
			}

			// could not find in kernel directly, check using resolvers
			return HasValidComponentFromResolver(dependency);
		}

		private bool HasValidComponent(String key, DependencyModel dependency)
		{
			if (IsValidHandlerState(kernel.GetHandler(key)))
				return true;
			// could not find in kernel directly, check using resolvers
			return HasValidComponentFromResolver(dependency);
		}

		private bool HasValidComponentFromResolver(DependencyModel dependency)
		{
			if (Kernel.Resolver.CanResolve(null, this, model, dependency))
				return true;
			return false;
		}

		private bool IsValidHandlerState(IHandler handler)
		{
			if (handler == null)
				return false;

			return handler.CurrentState == HandlerState.Valid;
		}

		private void AddGraphDependency(ComponentModel model)
		{
			ComponentModel.AddDependent(model);
		}

		private DependencyModel[] Union(ICollection<DependencyModel> firstset, ICollection<DependencyModel> secondset)
		{
			var result = new DependencyModel[firstset.Count + secondset.Count];

			firstset.CopyTo(result, 0);
			secondset.CopyTo(result, firstset.Count);

			return result;
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

		private void RaiseHandlerStateChanged()
		{
			if (OnHandlerStateChanged != null)
				OnHandlerStateChanged(this, EventArgs.Empty);
		}

		private void DisconnectEvents()
		{
			Kernel.HandlersChanged -= DependencySatisfied;
			Kernel.AddedAsChildKernel -= OnAddedAsChildKernel;
			OnHandlerStateChanged -= HandlerStateChanged;
		}
	}
}
