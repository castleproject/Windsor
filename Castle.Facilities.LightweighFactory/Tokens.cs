namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Collections;
	using System.Reflection;

	using Castle.MicroKernel;

	internal static class Tokens
	{
		internal static readonly MethodInfo HashtableAdd = typeof(Hashtable).GetMethod("Add");
		internal static readonly ConstructorInfo HashtableCtor = typeof(Hashtable).GetConstructor(Type.EmptyTypes);

		internal static readonly MethodInfo KernelResolve_IDictionary = typeof(IKernel).GetMethod("Resolve",
		                                                                                          new[]
		                                                                                          { typeof(IDictionary) });

		internal static readonly FieldInfo LightweightFactoryKernel = typeof(LightweightFactory).GetField("kernel",
		                                                                                                  BindingFlags.
		                                                                                                  	Instance |
		                                                                                                  BindingFlags.
		                                                                                                  	NonPublic);
	}
}