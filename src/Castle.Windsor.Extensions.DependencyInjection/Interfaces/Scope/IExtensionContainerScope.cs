namespace Castle.Windsor.Extensions.DependencyInjection.Interfaces.Scope
{
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;

	internal interface IExtensionContainerScope : ILifetimeScope
	{
		ExtensionContainerScope Current { get; }
		ExtensionContainerScope Root { get; }
		ExtensionContainerScope BeginScope();
	}
}