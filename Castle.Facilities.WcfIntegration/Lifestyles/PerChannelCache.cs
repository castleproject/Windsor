namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;
	using System.ServiceModel;

	public class PerChannelCache : AbstractWcfLifestyleCache<IContextChannel>
	{
		protected override void InitContext(IContextChannel context)
		{
			context.Faulted += Shutdown;
			context.Closed += Shutdown;
		}

		protected override void ShutdownContext(IContextChannel context)
		{
			context.Faulted -= Shutdown;
			context.Closed -= Shutdown;
		}

		private void Shutdown(object sender, EventArgs e)
		{
			ShutdownCache();
		}
	}
}