namespace Castle.Facilities.NHibernateIntegration
{
	using System;

	/// <summary>
	/// Tells to the facility that the class needs a valie Session.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class NHSessionAwareAttribute : Attribute
	{
	}

	/// <summary>
	/// Mark the methods that needs a Session
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class NHSessionRequiredAttribute : Attribute
	{
	}
}
