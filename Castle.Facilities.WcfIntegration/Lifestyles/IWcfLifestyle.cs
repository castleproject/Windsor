namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;

	using Castle.MicroKernel;

	/// <summary>
	/// Contract for managing object lifestyles in the context of WCF runtime.
	/// </summary>
	public interface IWcfLifestyle:ILifestyleManager
	{
		/// <summary>
		/// Id of the component associated with the lifestyle manager instance.
		/// This Id does not have to have anything to do with the Id of the component assigned by the container.
		/// It is used for internal tracking purposes os the facility.
		/// </summary>
		Guid ComponentId { get; }
	}
}