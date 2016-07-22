namespace CastleTests.TestImplementationsOfExtensionPoints
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;

	public class UseStringGenericStrategy : IGenericImplementationMatchingStrategy
	{
		public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
		{
			return new[] { typeof(string) };
		}
	}
}