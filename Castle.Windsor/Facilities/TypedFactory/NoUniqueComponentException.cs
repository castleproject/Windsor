namespace Castle.Facilities.LightweighFactory
{
	using System;

	using Castle.MicroKernel;

	public class NoUniqueComponentException : ComponentNotFoundException
	{
		public NoUniqueComponentException(Type service, string message)
			: base(service, message)
		{
		}
	}
}