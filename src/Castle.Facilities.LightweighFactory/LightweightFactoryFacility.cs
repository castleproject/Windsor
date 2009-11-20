namespace Castle.Facilities.LightweighFactory
{
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Resolvers;

	public class LightweightFactoryFacility : AbstractFacility
	{
		protected override void Init()
		{
			Kernel.Resolver.AddSubResolver(new ParametersBinder());
			Kernel.AddComponent("lightweight-factory", typeof(ILazyComponentLoader), typeof(LightweightFactory));
			Kernel.AddComponent("lightweight-factory-delegate-builder", typeof(IDelegateBuilder),
								typeof(ExpressionTreeBasedDelegateBuilder));
		}
	}
}