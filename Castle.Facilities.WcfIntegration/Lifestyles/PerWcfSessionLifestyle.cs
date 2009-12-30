namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle;

	/// <summary>
	/// Manages object instances in the context of WCF session. This means that when a component 
	/// with this lifestyle is requested multiple times during WCF session, the same instance will be provided.
	/// If no WCF session is available falls back to the default behavior of transient.
	/// </summary>
	public class PerWcfSessionLifestyle : AbstractLifestyleManager
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

		public override void Dispose()
		{

		}

		public override object Resolve(CreationContext context)
		{
			var operation = operationContextProvider.Current;
			if (operation == null || string.IsNullOrEmpty(operation.SessionId))
			{
				return base.Resolve(context);
			}

			var channel = operation.Channel;

			// TODO: does this need locking?
			var lifestyle = channel.Extensions.Find<PerChannelLifestyleExtension>();

			if (lifestyle == null)
			{
				lifestyle = new PerChannelLifestyleExtension();
				channel.Extensions.Add(lifestyle);
			}

			var component = lifestyle[this];
			if (component == null)
			{
				component = base.Resolve(context);
				lifestyle[this] = component;
			}

			return component;
		}
	}
}