using System.Diagnostics.Contracts;
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
		public static ResolveScope<T> ResolveScope<T>(this IWindsorContainer container)
			where T : class
		{
			Contract.Requires(container != null);
			return new ResolveScope<T>(container);
		}
		
		/// <summary>
		/// Resolve the service denoted by T. Beware that some of the components in the IO scope,
		/// namely the file and directory implementations are per-transaction and as such shouldn't be
		/// resolved unless there is an ambient transaction.
		/// </summary>
		/// <typeparam name="T">The service to resolve.</typeparam>
		/// <param name="container">The container to resolve from.</param>
		/// <returns>The IOResolveScope</returns>
		public static ResolveScope<T> ResolveIOScope<T>(this IWindsorContainer container)
			where T : class
		{
			Contract.Requires(container != null);
			return new IOResolveScope<T>(container);
		}
	}
}