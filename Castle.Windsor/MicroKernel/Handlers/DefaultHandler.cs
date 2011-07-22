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
	using System.Text;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.ModelBuilder.Inspectors;

	/// <summary>
	///   Summary description for DefaultHandler.
	/// </summary>
	[Serializable]
	public class DefaultHandler : AbstractHandler
	{
		/// <summary>
		///   Lifestyle manager instance
		/// </summary>
		protected ILifestyleManager lifestyleManager;

		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultHandler" /> class.
		/// </summary>
		/// <param name = "model"></param>
		public DefaultHandler(ComponentModel model) : base(model)
		{
		}

		public override void Dispose()
		{
			lifestyleManager.Dispose();
		}

		/// <summary>
		///   disposes the component instance (or recycle it)
		/// </summary>
		/// <param name = "burden"></param>
		/// <returns>true if destroyed</returns>
		public override bool ReleaseCore(Burden burden)
		{
			return lifestyleManager.Release(burden.Instance);
		}

		protected void AssertNotWaitingForDependency()
		{
			if (CurrentState == HandlerState.WaitingDependency)
			{
				var message = new StringBuilder("Can't create component '");
				message.Append(ComponentModel.Name);
				message.AppendLine("' as it has dependencies to be satisfied.");

				var inspector = new DependencyInspector(message);
				ObtainDependencyDetails(inspector);

				throw new HandlerException(inspector.Message, ComponentModel.ComponentName);
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
				case LifestyleType.Scoped:
					manager = new ScopedLifestyleManager(CreateScopeAccessor());
					break;
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
					manager = new ScopedLifestyleManager(new WebRequestScopeAccessor());
					break;
#endif
				case LifestyleType.Custom:
					manager = ComponentModel.CustomLifestyle.CreateInstance<ILifestyleManager>();

					break;
				case LifestyleType.Pooled:
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
					break;
				default:
					//this includes LifestyleType.Undefined, LifestyleType.Singleton and invalid values
					manager = new SingletonLifestyleManager();
					break;
			}

			manager.Init(activator, Kernel, ComponentModel);

			return manager;
		}

		protected override void InitDependencies()
		{
			var activator = Kernel.CreateComponentActivator(ComponentModel);
			lifestyleManager = CreateLifestyleManager(activator);

			var awareActivator = activator as IDependencyAwareActivator;
			if (awareActivator != null && awareActivator.CanProvideRequiredDependencies(ComponentModel))
			{
				return;
			}

			base.InitDependencies();
		}

		protected override object Resolve(CreationContext context, bool instanceRequired)
		{
			Burden burden;
			return ResolveCore(context, false, instanceRequired, out burden);
		}

		/// <summary>
		///   Returns an instance of the component this handler
		///   is responsible for
		/// </summary>
		/// <param name = "context"></param>
		/// <param name = "requiresDecommission"></param>
		/// <param name = "instanceRequired"></param>
		/// <param name = "burden"></param>
		/// <returns></returns>
		protected object ResolveCore(CreationContext context, bool requiresDecommission, bool instanceRequired, out Burden burden)
		{
			if (CanResolvePendingDependencies(context) == false)
			{
				if (instanceRequired == false)
				{
					burden = null;
					return null;
				}

				AssertNotWaitingForDependency();
			}
			using (var ctx = context.EnterResolutionContext(this, requiresDecommission))
			{
				var instance = lifestyleManager.Resolve(context, context.ReleasePolicy);
				burden = ctx.Burden;
				return instance;
			}
		}

		private IScopeAccessor CreateScopeAccessor()
		{
			var selector = (Func<IHandler[], IHandler>)ComponentModel.ExtendedProperties[Constants.ScopeRootSelector];
			if (selector == null)
			{
				return new LifetimeScopeAccessor();
			}
			return new CreationContextScopeAccessor(ComponentModel, selector);
		}
	}
}