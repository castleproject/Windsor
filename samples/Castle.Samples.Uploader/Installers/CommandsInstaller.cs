namespace Castle.Samples.Uploader.Installers
{
	using System;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Samples.Uploader.Commands;
	using Castle.Windsor;

	public class CommandsInstaller:IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			//container.Register(AllTypes.FromThisAssembly()
			//    .BasedOn<AbstractCommand>()
			//    .Configure(c=>c.OnCreate((k, a)=>a.Init()))
			//    .WithService.DefaultInterface()
			//    )
		}
	}
}