namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Reflection;

	using Castle.MicroKernel;

	public interface IDelegateBuilder
	{
		Delegate BuildDelegate(IHandler handler, MethodInfo invoke, Type service, LightweightFactory factory);
	}
}