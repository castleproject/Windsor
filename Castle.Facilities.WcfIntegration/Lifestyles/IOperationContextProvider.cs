namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System.ServiceModel;

	public interface IOperationContextProvider
	{
		OperationContext Current { get; }
	}
}