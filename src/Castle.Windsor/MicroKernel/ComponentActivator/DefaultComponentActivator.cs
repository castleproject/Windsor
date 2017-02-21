// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
    using System.Reflection;
#if FEATURE_SERIALIZATION
    using System.Runtime.Serialization;
#endif
	using System.Security;
#if FEATURE_SECURITY_PERMISSIONS
	using System.Security.Permissions;
#endif

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Context;

#if DOTNET35 || SILVERLIGHT
	using System.Linq;
#endif

    /// <summary>
    /// 	Standard implementation of <see cref = "IComponentActivator" />. Handles the selection of the best constructor, fills the writable properties the component exposes, run the commission and
    /// 	decommission lifecycles, etc.
    /// </summary>
    /// <remarks>
    /// 	Custom implementors can just override the <c>CreateInstance</c> method. Please note however that the activator is responsible for the proxy creation when needed.
    /// </remarks>
#if FEATURE_SERIALIZATION
	[Serializable]
#endif
    public class DefaultComponentActivator : AbstractComponentActivator
	{
#if FEATURE_SERIALIZATION
		private readonly bool useFastCreateInstance;
#endif

		/// <summary>
		/// 	Initializes a new instance of the <see cref = "DefaultComponentActivator" /> class.
		/// </summary>
		/// <param name = "model"> </param>
		/// <param name = "kernel"> </param>
		/// <param name = "onCreation"> </param>
		/// <param name = "onDestruction"> </param>
		public DefaultComponentActivator(ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
#if FEATURE_SECURITY_PERMISSIONS
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

			if (createProxy == false && Model.Implementation.GetTypeInfo().IsAbstract)
			{
				throw new ComponentRegistrationException(
					string.Format(
						"Type {0} is abstract.{2} As such, it is not possible to instansiate it as implementation of service '{1}'. Did you forget to proxy it?",
						Model.Implementation.FullName,
						Model.Name,
						Environment.NewLine));
			}

			var createInstance = true;
			if (createProxy)
			{
				createInstance = Kernel.ProxyFactory.RequiresTargetInstance(Kernel, Model);
			}

			if (createInstance)
			{
				instance = CreateInstanceCore(constructor, arguments, implType);
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
					throw new ComponentActivatorException("ComponentActivator: could not proxy " + Model.Implementation.FullName, ex, Model);
				}
			}

			return instance;
		}

		protected object CreateInstanceCore(ConstructorCandidate constructor, object[] arguments, Type implType)
		{
			object instance;
			try
			{
#if !FEATURE_SERIALIZATION
                instance = implType.CreateInstance<object>(arguments);
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
				if (ex is ComponentActivatorException)
				{
					throw;
				}

				throw new ComponentActivatorException(
					"ComponentActivator: could not instantiate " + Model.Implementation.FullName, ex, Model);
			}
			return instance;
		}

#if FEATURE_SERIALIZATION
#if !DOTNET35 && !CLIENTPROFILE
		[SecuritySafeCritical]
#endif
		private object FastCreateInstance(Type implType, object[] arguments, ConstructorCandidate constructor)
		{
			if (constructor == null || constructor.Constructor == null)
			{
				throw new ComponentActivatorException(
					string.Format(
						"Could not find a public constructor for type {0}.{1}" +
						"Windsor by default cannot instantiate types that don't expose public constructors.{1}" +
						"To expose the type as a service add public constructor, or use custom component activator.",
						implType,
						Environment.NewLine), Model);
			}
			var instance = FormatterServices.GetUninitializedObject(implType);

			constructor.Constructor.Invoke(instance, arguments);
			return instance;
		}
#endif

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
				if (BestScoreSoFar(candidatePoints, winnerPoints, winnerCandidate))
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
				throw new NoResolvableConstructorFoundException(Model.Implementation, Model);
			}

			return winnerCandidate;
		}

		private static bool BestScoreSoFar(int candidatePoints, int winnerPoints, ConstructorCandidate winnerCandidate)
		{
			return winnerCandidate == null || winnerPoints < candidatePoints;
		}

		private static bool BestPossibleScore(ConstructorCandidate candidate, int candidatePoints)
		{
			return candidatePoints == candidate.Dependencies.Length*100;
		}

		private bool CheckCtorCandidate(ConstructorCandidate candidate, CreationContext context, out int candidatePoints)
		{
			candidatePoints = 0;
			foreach (var dependency in candidate.Dependencies)
			{
				if (CanSatisfyDependency(context, dependency))
				{
					candidatePoints += 100;
				}
				else if (dependency.HasDefaultValue)
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

		protected virtual bool CanSatisfyDependency(CreationContext context, DependencyModel dependency)
		{
			return Kernel.Resolver.CanResolve(context, context.Handler, Model, dependency);
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
							"Error setting property {1}.{0} in component {2}. See inner exception for more information.{4}" +
							"If you don't want Windsor to set this property you can do it by either decorating it with {3} or via registration API.{4}" +
							"Alternatively consider making the setter non-public.",
							property.Property.Name,
							instance.GetType().Name,
							Model.Name,
							typeof(DoNotWireAttribute).Name,
							Environment.NewLine);
					throw new ComponentActivatorException(message, ex, Model);
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
				catch (Exception e)
				{
					if (property.Dependency.IsOptional == false)
					{
						throw;
					}
					Kernel.Logger.Warn(string.Format("Exception when resolving optional dependency {0} on component {1}.", property.Dependency, Model.Name), e);
				}
			}
			return null;
		}
	}
}