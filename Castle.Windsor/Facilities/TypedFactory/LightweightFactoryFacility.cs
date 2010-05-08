namespace Castle.Facilities.LightweighFactory
{
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Resolvers;

	public class LightweightFactoryFacility : AbstractFacility
	{
		public static readonly string FactoryKey = "lightweight-factory";
		public static readonly string DelegateBuilderKey = "lightweight-factory-delegate-builder";

		protected override void Init()
		{
			Kernel.Resolver.AddSubResolver(new ParametersBinder());
			if (!Kernel.HasComponent(FactoryKey))
			{
				Kernel.AddComponent(FactoryKey, typeof(ILazyComponentLoader), typeof(LightweightFactory));
			}
			if (!Kernel.HasComponent(DelegateBuilderKey))
			{
				Kernel.AddComponent(DelegateBuilderKey, typeof(IDelegateBuilder),
				                    typeof(ExpressionTreeBasedDelegateBuilder));
			}
		}
	}
}