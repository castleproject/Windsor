namespace Castle.Facilities.WcfIntegration
{
	using Castle.Facilities.WcfIntegration.Lifestyles;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Registration.Lifestyle;

	public static class LifestyleRegistrationExtensions
	{
		public static ComponentRegistration<S> PerWcfSession<S>(this LifestyleGroup<S> @group)
		{
			return group.Custom<PerWcfSessionLifestyle>();
		}

		public static ComponentRegistration<S> PerWcfOperation<S>(this LifestyleGroup<S> @group)
		{
			return group.Custom<PerWcfOperationLifestyle>();
		}
	}
}