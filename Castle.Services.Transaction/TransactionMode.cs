namespace Castle.Services.Transaction
{
	/// <summary>
	/// The supported transaction mode for the components.
	/// </summary>
	public enum TransactionMode
	{
		/// <summary>
		/// 
		/// </summary>
		Unspecified,

		/// <summary>
		/// transaction context will be created 
		/// managing internally a connection, no 
		/// transaction is opened though
		/// </summary>
		NotSupported,

		/// <summary>
		/// transaction context will be created if not present 
		/// </summary>
		Requires,

		/// <summary>
		/// a new transaction context will be created 
		/// </summary>
		RequiresNew,

		/// <summary>
		/// An existing appropriate transaction context 
		/// will be joined if present, but if if there is no current
		/// transaction on the thread, no transaction will be created.
		/// </summary>
		Supported
	}
}