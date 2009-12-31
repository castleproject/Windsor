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
			if (FirstPassChecks(key, service) == false)
			{
				return null;
			}

			MethodInfo invoke = GetInvokeMethod(service);
			if (!HasReturn(invoke))
			{
				return null;
			}

			string serviceName = GetServiceName(key);

			IHandler handler = GetHandler(invoke, serviceName);
			if (handler == null)
			{
				return null;
			}

			Delegate @delegate = delegateBuiler.BuildDelegate(handler, invoke, service, this);
			Debug.Assert(@delegate != null, "@delegate != null");
			return Component.For(service)
				.Named(key)
				.Instance(@delegate)
				.LifeStyle.Singleton;
		}

		#endregion

		private IHandler GetHandler(MethodInfo invoke, string serviceName)
		{
			IHandler handler = kernel.GetHandler(serviceName);
			if (handler == null)
			{
				IHandler[] handlers = kernel.GetAssignableHandlers(invoke.ReturnType);
				if (handlers.Length != 1)
				{
					handler = handlers.SingleOrDefault(h => h.ComponentModel.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					handler = handlers.Single();
				}
			}
			return handler;
		}

		private string GetServiceName(string key)
		{
			string serviceName;
			if (key.Equals("factory", StringComparison.OrdinalIgnoreCase))
			{
				serviceName = key;
			}
			else
			{
				serviceName = key.Substring(0, key.Length - "factory".Length);
			}
			return serviceName;
		}

		private bool FirstPassChecks(string key, Type service)
		{
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}

			if (service == null)
			{
				return false;
			}

			if (!key.EndsWith("Factory", StringComparison.OrdinalIgnoreCase))
			{
				//just a convention...
				return false;
			}

			if (!typeof(MulticastDelegate).IsAssignableFrom(service))
			{
				return false;
			}
			return true;
		}

		private bool HasReturn(MethodInfo invoke)
		{
			return invoke.ReturnType != typeof(void);
		}

		private MethodInfo GetInvokeMethod(Type @delegate)
		{
			MethodInfo invoke = @delegate.GetMethod("Invoke");
			Debug.Assert(invoke != null, "invoke != null");
			return invoke;
		}
	}
}