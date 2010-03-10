namespace Castle.Services.Transaction
{
	/// <summary>
	/// Isolation modes which the transactions can run in. These do not
	/// apply equally to FtX which run as ReadCommitted for all transactions.
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