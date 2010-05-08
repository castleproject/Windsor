namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	public class LightweightFactory : ILazyComponentLoader
	{
		private readonly IDelegateBuilder delegateBuiler;
		private readonly IKernel kernel;

		public LightweightFactory(IKernel kernel, IDelegateBuilder delegateBuiler)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			if (delegateBuiler == null)
			{
				throw new ArgumentNullException("delegateBuiler");
			}
			this.kernel = kernel;
			this.delegateBuiler = delegateBuiler;
		}

		#region ILazyComponentLoader Members

		public IRegistration Load(string key, Type service)
		{
			if (string.IsNullOrEmpty(key) || service == null)
			{
				return null;
			}

			if (!typeof(MulticastDelegate).IsAssignableFrom(service))
			{
				return null;
			}

			var invoke = GetInvokeMethod(service);
			if (!HasReturn(invoke))
			{
				return null;
			}

			if (ShouldLoad(key, service) == false)
			{
				return null;
			}

			var serviceName = ExtractServiceName(key);
			var handler = GetHandlerToBeResolvedByDelegate(invoke, serviceName);
			if (handler == null)
			{
				return null;
			}

			var @delegate = delegateBuiler.BuildDelegate(handler, invoke, service, this);
			Debug.Assert(@delegate != null, "@delegate != null");
			return Component.For(service)
				.Named(key)
				.Instance(@delegate)
				.LifeStyle.Singleton;
		}

		protected virtual string ExtractServiceName(string key)
		{
			return key;
		}

		protected virtual bool ShouldLoad(string key, Type service)
		{
			return true;
		}

		#endregion

		protected virtual IHandler GetHandlerToBeResolvedByDelegate(MethodInfo invoke, string serviceName)
		{
			if(!string.IsNullOrEmpty(serviceName))
			{
				var handler = kernel.GetHandler(serviceName);
				if (handler != null)
				{
					return handler;
				}
			}

			var handlers = kernel.GetAssignableHandlers(invoke.ReturnType);
			if (handlers.Length == 1)
			{
				return handlers.Single();
			}
			var potentialHandler = handlers.SingleOrDefault(h => h.ComponentModel.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
			if (potentialHandler == null)
			{
				throw new NoUniqueComponentException(invoke.ReturnType,
				                                     "Lightweight factory ({0}) was unable to uniquely nominate component to resolve for service '{1}'. " +
				                                     "You may provide your own selection logic, by registering custom LightweightFactory with key LightweightFactoryFacility.FactoryKey " +
				                                     "before registering the facility.");
			}

			return potentialHandler;
		}

		protected bool HasReturn(MethodInfo invoke)
		{
			return invoke.ReturnType != typeof(void);
		}

		protected MethodInfo GetInvokeMethod(Type @delegate)
		{
			MethodInfo invoke = @delegate.GetMethod("Invoke");
			Debug.Assert(invoke != null, "invoke != null");
			return invoke;
		}
	}
}