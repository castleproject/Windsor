namespace Castle.Facilities.NHibernateIntegration.Internal
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.Core.Logging;
	using Castle.DynamicProxy;

	/// <summary>
	/// Interceptor in charge o the automatic session management.
	/// </summary>
	[Transient]
	public class NHSessionInterceptor : Castle.DynamicProxy.IInterceptor, IOnBehalfAware
	{
		private readonly ISessionManager sessionManager;
		private IEnumerable<MethodInfo> metaInfo;

		/// <summary>
		/// Constructor
		/// </summary>
		public NHSessionInterceptor(ISessionManager sessionManager)
		{
			this.sessionManager = sessionManager;

			Logger = NullLogger.Instance;
		}

		/// <summary>
		/// Gets or sets the logger.
		/// </summary>
		/// <value>The logger.</value>
		public ILogger Logger { get; set; }

		#region IOnBehalfAware

		/// <summary>
		/// Sets the intercepted component's ComponentModel.
		/// </summary>
		/// <param name="target">The target's ComponentModel</param>
		public void SetInterceptedComponentModel(ComponentModel target)
		{
			metaInfo = (MethodInfo[]) target.ExtendedProperties[NHSessionComponentInspector.SessionRequiredMetaInfo];
		}

		#endregion

		/// <summary>
		/// Intercepts the specified invocation and creates a transaction
		/// if necessary.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		/// <returns></returns>
		public void Intercept(IInvocation invocation)
		{
			MethodInfo methodInfo;
			if (invocation.Method.DeclaringType.IsInterface)
				methodInfo = invocation.MethodInvocationTarget;
			else
				methodInfo = invocation.Method;

			if (metaInfo == null || !metaInfo.Contains(methodInfo))
			{
				invocation.Proceed();
				return;
			}

			var session = sessionManager.OpenSession();

			try
			{
				invocation.Proceed();
			}
			finally
			{
				session.Dispose();
			}
		}
	}
}