using System;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// Indicates that the target class wants to use
	/// the transactional services.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public sealed class TransactionalAttribute : System.Attribute
	{
	}
}