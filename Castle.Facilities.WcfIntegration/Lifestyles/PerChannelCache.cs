namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System.ServiceModel;

	public class PerChannelCache : AbstractWcfLifestyleCache<IContextChannel>
	{
		protected override void InitContext(IContextChannel context)
		{
		}

		protected override void ShutdownContext(IContextChannel context)
		{
		}

	}
}