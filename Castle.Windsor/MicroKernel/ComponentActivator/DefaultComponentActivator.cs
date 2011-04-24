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

namespace Castle.MicroKernel.ComponentActivator
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Security;
	using System.Security.Permissions;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;

#if DOTNET35 || SILVERLIGHT
	using System.Linq;
#endif

	/// <summary>
	///   Standard implementation of <see cref = "IComponentActivator" />.
	///   Handles the selection of the best constructor, fills the
	///   writable properties the component exposes, run the commission 
	///   and decommission lifecycles, etc.
	/// </summary>
	/// <remarks>
	///   Custom implementors can just override the <c>CreateInstance</c> method.
	///   Please note however that the activator is responsible for the proxy creation
	///   when needed.
	/// </remarks>
	[Serializable]
	public class DefaultComponentActivator : AbstractComponentActivator
	{
#if (!SILVERLIGHT)
		private readonly bool useFastCreateInstance;
#endif

		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultComponentActivator" /> class.
		/// </summary>
		/// <param name = "model"></param>
		/// <param name = "kernel"></param>
		/// <param name = "onCreation"></param>
		/// <param name = "onDestruction"></param>
		public DefaultComponentActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
#if (!SILVERLIGHT)
			useFastCreateInstance = !model.Implementation.IsContextful && new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).IsGranted();
#endif
		}

		protected override object InternalCreate(CreationContext context)
		{
			var instance = Instantiate(context);
			context.SetContextualProperty(this, instance);

			SetUpProperties(instance, context);

			ApplyCommissionConcerns(instance);

			return instance;
		}

		protected override void InternalDestroy(object instance)
		{
			ApplyDecommissionConcerns(instance);
		}

		protected virtual object Instantiate(CreationContext context)
		{
			var candidate = SelectEligibleConstructor(context);

			var arguments = CreateConstructorArguments(candidate, context);

			return CreateInstance(context, candidate, arguments);
		}

		protected virtual object CreateInstance(CreationContext context, ConstructorCandidate constructor, object[] arguments)
		{
			object instance = null;

			var implType = Model.Implementation;

			var createProxy = Kernel.ProxyFactory.ShouldCreateProxy(Model);
			var createInstance = true;

			if (createProxy == false && Model.Implementation.IsAbstract)
			{
				throw new ComponentRegistrationException(
					string.Format(
						"Type {0} is abstract.{2} As such, it is not possible to instansiate it as implementation of service '{1}'. Did you forget to proxy it?",
						Model.Implementation.FullName,
						Model.Name,
						Environment.NewLine));
			}

			if (createProxy)
			{
				createInstance = Kernel.ProxyFactory.RequiresTargetInstance(Kernel, Model);
			}

			if (createInstance)
			{
				try
				{
#if (SILVERLIGHT)
					instance = ReflectionUtil.CreateInstance<object>(implType, arguments);
#else
					if (useFastCreateInstance)
					{
						instance = FastCreateInstance(implType, arguments, constructor);
					}
					else
					{
						instance = implType.CreateInstance<object>(arguments);
					}
#endif
				}
				catch (Exception ex)
				{
					if (arguments != null)
					{
						foreach (var argument in arguments)
						{
							Kernel.ReleaseComponent(argument);
						}
					}

					throw new ComponentActivatorException(
						"ComponentActivator: could not instantiate " + Model.Implementation.FullName, ex);
				}
			}

			if (createProxy)
			{
				try
				{
					instance = Kernel.ProxyFactory.Create(Kernel, instance, Model, context, arguments);
				}
				catch (Exception ex)
				{
					if (arguments != null)
					{
						foreach (var argument in arguments)
						{
							Kernel.ReleaseComponent(argument);
						}
					}
					throw new ComponentActivatorException("ComponentActivator: could not proxy " + Model.Implementation.FullName, ex);
				}
			}

			return instance;
		}

#if (!SILVERLIGHT)
#if DOTNET40
		[SecuritySafeCritical]
#endif
		private static object FastCreateInstance(Type implType, object[] arguments, ConstructorCandidate constructor)
		{
			if (constructor == null || constructor.Constructor == null)
			{
				throw new ComponentActivatorException(
					string.Format(
						"Could not find a public constructor for type {0}. Windsor can not instantiate types that don't expose public constructors. To expose the type as a service add public constructor, or use custom component activator.",
						implType));
			}
			var instance = FormatterServices.GetUninitializedObject(implType);

			constructor.Constructor.Invoke(instance, arguments);
			return instance;
		}
#endif

		protected virtual void ApplyCommissionConcerns(object instance)
		{
			if (Model.Lifecycle.HasCommissionConcerns == false)
			{
				return;
			}

			instance = ProxyUtil.GetUnproxiedInstance(instance);
			ApplyConcerns(Model.Lifecycle.CommissionConcerns
#if DOTNET35 || SILVERLIGHT
				.ToArray()
#endif
			              , instance);
		}

		protected virtual void ApplyDecommissionConcerns(object instance)
		{
			if (Model.Lifecycle.HasDecommissionConcerns == false)
			{
				return;
			}

			instance = ProxyUtil.GetUnproxiedInstance(instance);
			ApplyConcerns(Model.Lifecycle.DecommissionConcerns
#if DOTNET35 || SILVERLIGHT
				.ToArray()
#endif
			              , instance);
		}

		protected virtual void ApplyConcerns(IEnumerable<ILifecycleConcern> steps, object instance)
		{
			foreach (var concern in steps)
			{
				concern.Apply(Model, instance);
			}
		}

		protected virtual ConstructorCandidate SelectEligibleConstructor(CreationContext context)
		{
			if (Model.Constructors.Count == 0)
			{
				// This is required by some facilities
				return null;
			}

			if (Model.Constructors.Count == 1)
			{
				return Model.Constructors[0];
			}
			ConstructorCandidate winnerCandidate = null;
			var winnerPoints = 0;
			foreach (var candidate in Model.Constructors)
			{
				int candidatePoints;
				if (CheckCtorCandidate(candidate, context, out candidatePoints) == false)
				{
					continue;
				}
				if (winnerCandidate == null || winnerPoints < candidatePoints)
				{
					if (BestPossibleScore(candidate, candidatePoints))
					{
						//since the constructors are sorted greedier first, we know there's no way any other .ctor is going to beat us here
						return candidate;
					}
					winnerCandidate = candidate;
					winnerPoints = candidatePoints;
				}
			}

			if (winnerCandidate == null)
			{
				throw new NoResolvableConstructorFoundException(Model.Implementation);
			}

			return winnerCandidate;
		}

		private bool BestPossibleScore(ConstructorCandidate candidate, int candidatePoints)
		{
			return candidatePoints == candidate.Dependencies.Length*100;
		}

		private bool CheckCtorCandidate(ConstructorCandidate candidate, CreationContext context, out int candidatePoints)
		{
			candidatePoints = 0;
			foreach (var dep in candidate.Dependencies)
			{
				if (CanSatisfyDependency(context, dep))
				{
					candidatePoints += 100;
				}
				else if (dep.HasDefaultValue)
				{
					candidatePoints += 1;
				}
				else
				{
					candidatePoints = 0;
					return false;
				}
			}
			return true;
		}

		protected virtual bool CanSatisfyDependency(CreationContext context, DependencyModel dep)
		{
			return Kernel.Resolver.CanResolve(context, context.Handler, Model, dep);
		}

		protected virtual object[] CreateConstructorArguments(ConstructorCandidate constructor, CreationContext context)
		{
			if (constructor == null)
			{
				return null;
			}

			var dependencyCount = constructor.Dependencies.Length;
			if (dependencyCount == 0)
			{
				return null;
			}

			var arguments = new object[dependencyCount];
			try
			{
				for (var i = 0; i < dependencyCount; i++)
				{
					arguments[i] = Kernel.Resolver.Resolve(context, context.Handler, Model, constructor.Dependencies[i]);
				}
				return arguments;
			}
			catch
			{
				foreach (var argument in arguments)
				{
					Kernel.ReleaseComponent(argument);
				}
				throw;
			}
		}

		protected virtual void SetUpProperties(object instance, CreationContext context)
		{
			instance = ProxyUtil.GetUnproxiedInstance(instance);
			var resolver = Kernel.Resolver;
			foreach (var property in Model.Properties)
			{
				var value = ObtainPropertyValue(context, property, resolver);
				if (value == null)
				{
					continue;
				}

				var setMethod = property.Property.GetSetMethod();
				try
				{
					setMethod.Invoke(instance, new[] { value });
				}
				catch (Exception ex)
				{
					var message =
						String.Format(
							"Error setting property {0} on type {1}, Component id is {2}. See inner exception for more information.",
							setMethod.Name, instance.GetType().FullName, Model.Name);
					throw new ComponentActivatorException(message, ex);
				}
			}
		}

		private object ObtainPropertyValue(CreationContext context, PropertySet property, IDependencyResolver resolver)
		{
			if (property.Dependency.IsOptional == false ||
			    resolver.CanResolve(context, context.Handler, Model, property.Dependency))
			{
				try
				{
					return resolver.Resolve(context, context.Handler, Model, property.Dependency);
				}
				catch (Exception)
				{
					if (property.Dependency.IsOptional == false)
					{
						throw;
					}
				}
			}
			return null;
		}
	}
}