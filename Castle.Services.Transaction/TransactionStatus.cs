namespace Castle.Services.Transaction
{
	/// <summary>
	/// 
	/// </summary>
	public enum TransactionStatus
	{
		NoTransaction,
		Active,
		Committed,
		RolledBack,
		Invalid
	}
}