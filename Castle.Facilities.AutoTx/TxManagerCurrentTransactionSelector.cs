using System;
using Castle.MicroKernel;

namespace Castle.Facilities.AutoTx
{
	/// <summary>
	/// v3.1: This selector will allow you to select the transient component if
	/// there is a component which is the same service and that is per-transaction.
	/// </summary>
	internal class TxManagerCurrentTransactionSelector : IHandlerSelector
	{
		public bool HasOpinionAbout(string key, Type service)
		{
			throw new NotImplementedException();
		}

		public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
		{
			throw new NotImplementedException();
		}
	}
}