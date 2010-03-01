namespace Castle.Services.Transaction
{
	/// <summary>
	/// The supported isolation modes.
	/// </summary>
	public enum IsolationMode
	{
		Unspecified,
		Chaos,
		ReadCommitted,
		ReadUncommitted,
		RepeatableRead,
		Serializable
	}
}