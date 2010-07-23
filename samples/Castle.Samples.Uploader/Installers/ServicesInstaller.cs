namespace Castle.Samples.Uploader.Installers
{
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;

	public class ServicesInstaller:IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(AllTypes.FromThisAssembly()
			                   	.Where(Component.IsInNamespace("Castle.Samples.Uploader.Services"))
			                   	.WithService.DefaultInterface());
		}
	}
}