namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;
	using System.ServiceModel;

	/// <summary>
	/// Manages object instances in the context of WCF session. This means that when a component 
	/// with this lifestyle is requested multiple times during WCF session, the same instance will be provided.
	/// If no WCF session is available falls back to the default behavior of transient.
	/// </summary>
	public class PerWcfSessionLifestyle : AbstractWcfLifestyleManager<IContextChannel, PerChannelCache>
	{
		private readonly IOperationContextProvider operationContextProvider;

		public PerWcfSessionLifestyle()
			: this(new OperationContextProvider())
		{
		}

		public PerWcfSessionLifestyle(IOperationContextProvider operationContextProvider)
		{
			if (operationContextProvider == null)
			{
				throw new ArgumentNullException("operationContextProvider");
			}

			this.operationContextProvider = operationContextProvider;
		}

		protected override IContextChannel GetCacheHolder()
		{
			var operation = operationContextProvider.Current;
			if (operation == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(operation.SessionId))
			{
				return null;
			}

			return operation.Channel;
		}
	}
}