namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System.ServiceModel;

	public class OperationContextProvider : IOperationContextProvider
	{
		public OperationContext Current
		{
			get { return OperationContext.Current; }
		}
	}
}