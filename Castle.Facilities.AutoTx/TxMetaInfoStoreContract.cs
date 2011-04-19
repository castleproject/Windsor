using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx
{
	[ContractClassFor(typeof (ITxMetaInfoStore))]
	internal abstract class TxMetaInfoStoreContract : ITxMetaInfoStore
	{
		Maybe<TxClassMetaInfo> ITxMetaInfoStore.GetMetaFromType(Type implementation)
		{
			Contract.Requires(implementation != null);
			var metaFromType = Contract.Result<Maybe<TxClassMetaInfo>>();
			Contract.Ensures(metaFromType != null,
			                 "the meta-info contract must return a non-null maybe (the maybe wraps null, also, so this should be an easy check)");
			return metaFromType;
		}
	}
}