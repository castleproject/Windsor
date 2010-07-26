namespace Castle.Samples.Uploader.WindsorExtensions
{
	using System;
	using System.Configuration;

	using Castle.Components.DictionaryAdapter;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	public class ConfigLoader:ILazyComponentLoader
	{
		private readonly Predicate<Type> isInConfigNamespace = Component.IsInNamespace("Castle.Samples.Uploader.Config",
		                                                                      includeSubnamespaces: true);

		public IRegistration Load(string key, Type service)
		{
			if (!IsConfig(service))
			{
				return null;
			}
			return Component.For(service)
				.UsingFactoryMethod(
					k => k.Resolve<IDictionaryAdapterFactory>()
					     	// generally we should pay attention to release the IDictionaryAdapterFactory
					     	// but since it's a singleton we can skip this step here.
					     	.GetAdapter(service, ConfigurationManager.AppSettings));
		}

		private bool IsConfig(Type service)
		{
			return service != null &&
			       service.IsInterface &&
			       isInConfigNamespace(service);
		}
	}
}