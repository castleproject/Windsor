using Castle.Windsor;

namespace Castle.Facilities.AutoTx.Testing
{
	/// <summary>
	/// Helper class for adding 'nifty' extensions to Windsor which ensures disposal/release of
	/// resources.
	/// </summary>
	public static class ResolveEx
	{
		/// <summary>
		/// Resolve the service denoted by T.
		/// </summary>
		/// <typeparam name="T">The service to resolve.</typeparam>
		/// <param name="container">The container to resolve from.</param>
		/// <returns>The IOResolveScope</returns>
		public static IOResolveScope<T> ResolveScope<T>(this IWindsorContainer container)
			where T : class
		{
			return new IOResolveScope<T>(container);
		}
	}
}