using System;

namespace Castle.Facilities.AutoTx
{
	/// <summary>
	/// Tells the kernel to give the transaction instace to the method as a parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class InjectTransactionAttribute : Attribute
	{
	}
}