namespace Castle.Windsor.Extensions.DependencyInjection
{
	/// <summary>
	/// Global settins to change the dependency injection behavior.
	/// These settings should be set before the container is created.
	/// </summary>
	public static class WindsorDependencyInjectionOptions
	{
		/// <summary>
		/// Map NetStatic lifestyle to Castle Windsor Singleton lifestyle.
		/// The whole RootScope handling is disabled.
		/// (defaut: false)
		/// </summary>
		public static bool MapNetStaticToSingleton { get; set; }
	}
}
